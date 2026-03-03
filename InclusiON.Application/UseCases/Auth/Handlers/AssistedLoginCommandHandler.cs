using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;
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

            var isAuthorized = await IsAuthorizedSupervisorAsync(person, supervisor.Id, cancellationToken);

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
                "Nuevo login asistido",
                SuccessMessages.AssistedLoginSuccessful,
                cancellationToken);
        }

        private async Task<bool> IsAuthorizedSupervisorAsync(
            PersonWithDisability person,
            Guid supervisorUserId,
            CancellationToken cancellationToken)
        {
            if (person.SupervisorUserId.HasValue && person.SupervisorUserId.Value == supervisorUserId)
            {
                return true;
            }

            var supervisorUser = await _identityService.FindByIdAsync(supervisorUserId);
            if (supervisorUser == null)
            {
                _logger.LogWarning("Supervisor user not found: {SupervisorUserId}", supervisorUserId);
                return false;
            }

            var roles = await _identityService.GetRolesAsync(supervisorUser);

            // Paralelizar lookups independientes de profesional y familiar
            var professionalTask = _repository.GetProfessionalByUserIdAsync(supervisorUserId, cancellationToken);
            var familyTask = _repository.GetFamilyByUserIdAsync(supervisorUserId, cancellationToken);
            await Task.WhenAll(professionalTask, familyTask);

            var professional = professionalTask.Result;
            if (professional != null && roles.Contains("Professional"))
            {
                return true;
            }

            var family = familyTask.Result;
            if (family != null && roles.Contains("Family"))
            {
                return true;
            }

            return false;
        }
    }
}
