using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Application.Auditing;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminDeactivateUserCommandHandler : ICommandHandler<AdminDeactivateUserCommand, ApiResponse<object>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IReportsRepository _reportsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDeactivateUserCommandHandler> _logger;
        private readonly IAccessAuditLogger _audit;
        private readonly IDateTimeProvider _dateTime;

        public AdminDeactivateUserCommandHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokensRepository,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IFamilyRepository familyRepository,
            IReportsRepository reportsRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminDeactivateUserCommandHandler> logger,
            IAccessAuditLogger audit,
            IDateTimeProvider dateTime)
        {
            _identityService = identityService;
            _refreshTokensRepository = refreshTokensRepository;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _familyRepository = familyRepository;
            _reportsRepository = reportsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _audit = audit;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            AdminDeactivateUserCommand command, CancellationToken cancellationToken)
        {
            if (command.UserId == command.RequestedByUserId)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.CannotDeactivateSelf,
                    "No puede desactivar su propia cuenta.");
            }

            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user is null)
                return ApiResponse<object>.NotFound("Usuario");

            if (!user.IsActive)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.UserAlreadyInactive,
                    "El usuario ya se encuentra inactivo.");
            }

            var roles = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            if (primaryRole == RoleNames.Professional)
            {
                var pro = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                if (pro is not null)
                {
                    var pendingReportsCount = await _reportsRepository.GetPendingReportsCountByProfessionalAsync(pro.Id, cancellationToken);
                    if (pendingReportsCount > 0)
                    {
                        return ApiResponse<object>.Conflict(
                            ErrorCode.HasPendingReports,
                            "Este profesional tiene informes pendientes. Debe reasignarlos o finalizarlos antes de proceder con la baja.");
                    }
                }

                var dependentPersonsCount = await _professionalsRepository.GetDependentAssistedLoginPersonsCountAsync(user.Id, cancellationToken);

                if (dependentPersonsCount > 0)
                {
                    return ApiResponse<object>.ErrorResult(
                        ErrorCode.InvalidOperation,
                        $"No se puede desactivar al profesional porque es el supervisor exclusivo de inicio de sesión asistido para {dependentPersonsCount} alumno(s). Reasigne la supervisión de estos alumnos antes de proceder.");
                }
            }

            var suspendedStudents = new List<string>();

            user.IsActive = false;
            await _identityService.UpdateUserAsync(user);

            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                user.Id, Constants.RevokeReasons.UserDeactivated, cancellationToken);

            await SetLinkedEntityActiveAsync(user, false, suspendedStudents, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} ({Email}) deactivated by admin {AdminId}",
                user.Id, user.Email, command.RequestedByUserId);

            await _audit.LogAsync(new AccessAuditEntry
            {
                UserId         = command.RequestedByUserId,
                ActionType     = AccessAuditValues.Action.Update,
                Result         = AccessAuditValues.Result.Allowed,
                AffectedTable  = "Users",
                AffectedRecordId = user.Id.ToString(),
                Details        = "Admin deactivated user account",
            }, cancellationToken);

            var successMessage = "Usuario desactivado exitosamente.";
            if (suspendedStudents.Count > 0)
            {
                successMessage += $" Se ha suspendido el acceso de los siguientes alumnos por no contar con otros representantes familiares activos: {string.Join(", ", suspendedStudents)}.";
            }

            return ApiResponse<object>.SuccessResult(successMessage);
        }

        private async Task SetLinkedEntityActiveAsync(User user, bool isActive, List<string> suspendedStudents, CancellationToken cancellationToken)
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

                        if (!isActive)
                        {
                            await _professionalsRepository.DeactivateAssignmentsAndCancelActivitiesAsync(user.Id, cancellationToken);
                        }
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

                        if (!isActive)
                        {
                            var suspended = await _familyRepository.DeactivateRepresentativeAndSuspendDependentStudentsAsync(user.Id, _dateTime.UtcNow, cancellationToken);
                            if (suspended is not null)
                            {
                                suspendedStudents.AddRange(suspended);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
