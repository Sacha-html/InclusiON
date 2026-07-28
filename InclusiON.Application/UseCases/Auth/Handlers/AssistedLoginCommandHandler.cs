using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;
using InclusiON.Domain.Enums;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class AssistedLoginCommandHandler : ICommandHandler<AssistedLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly ILoginSessionService _loginSessionService;
        private readonly ILogger<AssistedLoginCommandHandler> _logger;

        public AssistedLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            ILoginSessionService loginSessionService,
            ILogger<AssistedLoginCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _loginSessionService = loginSessionService;
            _logger = logger;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            AssistedLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

            var supervisor = await _identityService.FindByEmailAsync(command.SupervisorEmail.ToLower().Trim());

            if (supervisor == null)
            {
                return ApiResponse<VisualLoginResponse>.SuccessResult(
                    new VisualLoginResponse
                    {
                        Success = false,
                        ErrorMessage = ErrorMessages.SupervisorInvalidCredentials
                    });
            }

            if (!supervisor.IsActive)
            {
                return ApiResponse<VisualLoginResponse>.SuccessResult(
                    new VisualLoginResponse
                    {
                        Success = false,
                        ErrorMessage = ErrorMessages.AccountDeactivated
                    });
            }

            // Check if profile itself is active/approved
            var roles = await _identityService.GetRolesAsync(supervisor);
            if (roles != null)
            {
                if (roles.Contains(RoleNames.Professional))
                {
                    var professional = await _repository.GetProfessionalByUserIdAsync(supervisor.Id, cancellationToken);
                    if (professional != null && !professional.IsActive)
                    {
                        return ApiResponse<VisualLoginResponse>.SuccessResult(
                            new VisualLoginResponse
                            {
                                Success = false,
                                ErrorMessage = ErrorMessages.AccountDeactivated
                            });
                    }
                }
                else if (roles.Contains(RoleNames.Family))
                {
                    var family = await _repository.GetFamilyByUserIdAsync(supervisor.Id, cancellationToken);
                    if (family != null && family.Status != FamilyStatusEnum.Active)
                    {
                        return ApiResponse<VisualLoginResponse>.SuccessResult(
                            new VisualLoginResponse
                            {
                                Success = false,
                                ErrorMessage = ErrorMessages.AccountDeactivated
                            });
                    }
                }
            }

            var isAuthorized = await IsAuthorizedSupervisorAsync(person, supervisor, cancellationToken);

            if (!isAuthorized)
            {
                _logger.LogWarning(
                    "Intento de login asistido no autorizado. Persona: {PersonId}, Supervisor: {SupervisorId}",
                    command.UserId, supervisor.Id);

                return ApiResponse<VisualLoginResponse>.SuccessResult(
                    new VisualLoginResponse
                    {
                        Success = false,
                        ErrorMessage = ErrorMessages.SupervisorNotAuthorized
                    });
            }

            var signInStatus = await _identityService.CheckPasswordAsync(
                supervisor,
                command.SupervisorPassword,
                lockoutOnFailure: true);

            if (signInStatus != SignInStatus.Success)
            {
                if (signInStatus == SignInStatus.LockedOut)
                {
                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            IsLocked = true,
                            ErrorMessage = ErrorMessages.SupervisorAccountLocked
                        });
                }

                return ApiResponse<VisualLoginResponse>.SuccessResult(
                    new VisualLoginResponse
                    {
                        Success = false,
                        ErrorMessage = ErrorMessages.SupervisorInvalidCredentials
                    });
            }

            _logger.LogInformation(
                "Login asistido exitoso. Persona: {PersonId}, Supervisor: {SupervisorId}",
                person.User.Id, supervisor.Id);

            return await _loginSessionService.CreateVisualLoginSessionAsync(
                person.User,
                person,
                1, // Sesion asistida de 1 dia
                command.DeviceId,
                false, // No recordar dispositivo en login asistido
                Constants.RevokeReasons.NewLogin,
                SuccessMessages.AssistedLoginSuccessful,
                cancellationToken);
        }

        private async Task<bool> IsAuthorizedSupervisorAsync(
            PersonWithDisability person,
            User supervisorUser,
            CancellationToken cancellationToken)
        {
            var supervisorUserId = supervisorUser.Id;
            if (person.SupervisorUserId.HasValue && person.SupervisorUserId.Value == supervisorUserId)
            {
                // Verify the primary supervisor entity is active/approved based on their role
                var directRoles = await _identityService.GetRolesAsync(supervisorUser);
                if (directRoles.Contains(RoleNames.Professional))
                {
                    var professional = await _repository.GetProfessionalByUserIdAsync(supervisorUserId, cancellationToken);
                    if (professional == null || !professional.IsActive || professional.Status != ProfessionalStatusEnum.Approved)
                    {
                        return false;
                    }
                }
                else if (directRoles.Contains(RoleNames.Family))
                {
                    var family = await _repository.GetFamilyByUserIdAsync(supervisorUserId, cancellationToken);
                    if (family == null || family.Status != FamilyStatusEnum.Active)
                    {
                        return false;
                    }
                }
                return true;
            }

            var roles = await _identityService.GetRolesAsync(supervisorUser);

            // DbContext no es thread-safe: consultar secuencialmente y solo la tabla
            // correspondiente al rol detectado.
            // Se verifica ademas el vinculo activo a ESTA persona con CanSuperviseLogin (HU-IN-172).
            if (roles.Contains(RoleNames.Professional))
            {
                var professional = await _repository.GetProfessionalByUserIdAsync(supervisorUserId, cancellationToken);
                if (professional != null
                    && professional.IsActive
                    && professional.Status == ProfessionalStatusEnum.Approved
                    && await _repository.CanProfessionalSupervisedLoginAsync(professional.Id, person.Id, cancellationToken))
                {
                    return true;
                }
            }

            if (roles.Contains(RoleNames.Family))
            {
                var family = await _repository.GetFamilyByUserIdAsync(supervisorUserId, cancellationToken);
                if (family != null
                    && family.Status == FamilyStatusEnum.Active
                    && await _repository.CanFamilySupervisedLoginAsync(family.Id, person.Id, cancellationToken))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
