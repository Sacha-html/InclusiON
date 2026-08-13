using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Auditing;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminReactivateUserCommandHandler : ICommandHandler<AdminReactivateUserCommand, ApiResponse<ResetPasswordResultResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminReactivateUserCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IAccessAuditLogger _audit;

        public AdminReactivateUserCommandHandler(
            IIdentityService identityService,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IFamilyRepository familyRepository,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            ILogger<AdminReactivateUserCommandHandler> logger,
            IDateTimeProvider dateTime,
            IAccessAuditLogger audit)
        {
            _identityService = identityService;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _familyRepository = familyRepository;
            _backgroundJobs = backgroundJobs;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _audit = audit;
        }

        public async Task<ApiResponse<ResetPasswordResultResponse>> HandleAsync(
            AdminReactivateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user is null)
                return ApiResponse<ResetPasswordResultResponse>.NotFound("Usuario");

            if (user.IsActive)
            {
                return ApiResponse<ResetPasswordResultResponse>.ErrorResult(
                    ErrorCode.UserAlreadyActive,
                    "El usuario ya se encuentra activo.");
            }

            var tempPassword = PasswordGenerator.GenerateTemporary();

            var (succeeded, errors) = await _identityService.ResetPasswordAsync(user, tempPassword);
            if (!succeeded)
            {
                return ApiResponse<ResetPasswordResultResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    $"Error al generar contraseña: {string.Join(", ", errors)}");
            }

            user.IsActive = true;
            user.MustChangePassword = true;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            await _identityService.UpdateUserAsync(user);

            await SetLinkedEntityActiveAsync(user, true, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} ({Email}) reactivated by admin {AdminId}",
                user.Id, user.Email, command.RequestedByUserId);

            await _audit.LogAsync(new AccessAuditEntry
            {
                UserId           = command.RequestedByUserId,
                ActionType       = AccessAuditValues.Action.Update,
                Result           = AccessAuditValues.Result.Allowed,
                AffectedTable    = "Users",
                AffectedRecordId = user.Id.ToString(),
                Details          = "Admin reactivated user account",
            }, cancellationToken);

            if (!string.IsNullOrEmpty(user.Email))
            {
                await _backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To           = user.Email,
                        Subject      = "Tu cuenta ha sido reactivada — InclusiON",
                        TemplateName = "AccountReactivated",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "UserName", user.Name ?? "Usuario" },
                            { "TemporaryPassword", tempPassword },
                            { "Year", _dateTime.UtcNow.Year.ToString() }
                        }
                    }),
                    maxRetries: 2,
                    cancellationToken: cancellationToken);
            }

            return ApiResponse<ResetPasswordResultResponse>.SuccessResult(
                new ResetPasswordResultResponse
                {
                    UserEmail = user.Email ?? string.Empty,
                    TemporaryPassword = tempPassword
                },
                "Usuario reactivado exitosamente. Se enviaron las credenciales por email.");
        }

        private async Task SetLinkedEntityActiveAsync(User user, bool isActive, CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            switch (primaryRole)
            {
                case RoleNames.Professional:
                    var pro = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (pro is not null)
                    {
                        pro.User.IsActive = isActive;
                        await _professionalsRepository.UpdateAsync(pro, cancellationToken);
                    }
                    break;

                case RoleNames.PersonWithDisability:
                    var person = await _personsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (person is not null)
                    {
                        person.User.IsActive = isActive;
                        await _personsRepository.UpdateAsync(person, cancellationToken);
                    }
                    break;

                case RoleNames.FamilyRepresentative:
                    var family = await _familyRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (family is not null)
                    {
                        family.User.IsActive = isActive;
                        await _familyRepository.UpdateAsync(family, cancellationToken);
                    }
                    break;
            }
        }
    }
}
