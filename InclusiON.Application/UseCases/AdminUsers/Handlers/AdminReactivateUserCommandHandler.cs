using Microsoft.Extensions.Logging;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminReactivateUserCommandHandler : ICommandHandler<AdminReactivateUserCommand, ApiResponse<ResetPasswordResultResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminReactivateUserCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public AdminReactivateUserCommandHandler(
            IIdentityService identityService,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IFamilyRepository familyRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<AdminReactivateUserCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _identityService = identityService;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _familyRepository = familyRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
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
            await _identityService.UpdateUserAsync(user);

            await SetLinkedEntityActiveAsync(user, true, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} ({Email}) reactivated by admin {AdminId}",
                user.Id, user.Email, command.RequestedByUserId);

            // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
            // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
            // Enviar email de reactivación (si el usuario tiene email)
            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendTemplatedEmailAsync(
                        user.Email,
                        "Tu cuenta ha sido reactivada — InclusiON",
                        "AccountReactivated",
                        new Dictionary<string, string?>
                        {
                            { "UserName", user.Name ?? "Usuario" },
                            { "TemporaryPassword", tempPassword },
                            { "Year", _dateTime.UtcNow.Year.ToString() }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar email de reactivación a {Email}", user.Email);
                }
            }

            return ApiResponse<ResetPasswordResultResponse>.SuccessResult(
                new ResetPasswordResultResponse
                {
                    UserEmail = user.Email ?? string.Empty
                },
                "Usuario reactivado exitosamente. Se enviaron las credenciales por email.");
        }

        private async Task SetLinkedEntityActiveAsync(User user, bool isActive, CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            switch (primaryRole)
            {
                case "Professional":
                    var pro = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (pro is not null)
                    {
                        pro.User.IsActive = isActive;
                        await _professionalsRepository.UpdateAsync(pro, cancellationToken);
                    }
                    break;

                case "PersonWithDisability":
                    var person = await _personsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (person is not null)
                    {
                        person.User.IsActive = isActive;
                        await _personsRepository.UpdateAsync(person, cancellationToken);
                    }
                    break;

                case "FamilyRepresentative":
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
