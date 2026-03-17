using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    /// <summary>
    /// Handler para actualizar el metodo de login de una persona con discapacidad.
    /// </summary>
    public class UpdateLoginMethodCommandHandler : ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateLoginMethodCommandHandler> _logger;

        // Constantes para IDs de metodos de login
        private const int LoginMethodStandard = 1;
        private const int LoginMethodPin = 2;
        private const int LoginMethodAssisted = 3;

        public UpdateLoginMethodCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ILogger<UpdateLoginMethodCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<UpdateLoginMethodResponse>> HandleAsync(
            UpdateLoginMethodCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Verificar que la persona existe
            var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);
            if (person == null)
            {
                return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Verificar que el metodo de login es valido y activo
            var loginMethod = await _repository.GetLoginMethodByIdAsync(command.LoginMethodId, cancellationToken);
            if (loginMethod == null)
            {
                return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                    ErrorCode.ResourceNotFound,
                    ErrorMessages.LoginMethodNotFound);
            }

            if (!loginMethod.IsActive)
            {
                return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                    ErrorCode.LoginMethodNotAllowed,
                    ErrorMessages.LoginMethodNotAvailable);
            }

            // Validaciones segun el metodo de login
            string? pinHash = null;
            Guid? supervisorUserId = null;
            string? temporaryPassword = null;

            switch (command.LoginMethodId)
            {
                case LoginMethodPin:
                    if (string.IsNullOrEmpty(command.Pin))
                    {
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.RequiredField,
                            ErrorMessages.PinRequiredForMethod);
                    }
                    if (command.Pin.Length < 4 || command.Pin.Length > 6 || !command.Pin.All(char.IsDigit))
                    {
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.InvalidFormat,
                            ErrorMessages.PinInvalidFormat);
                    }
                    pinHash = _passwordHasher.HashPassword(command.Pin);
                    break;

                case LoginMethodAssisted:
                    if (!command.SupervisorUserId.HasValue)
                    {
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.RequiredField,
                            ErrorMessages.SupervisorRequiredForAssisted);
                    }
                    // Verificar que el supervisor existe y es profesional o familiar
                    var supervisor = await _repository.GetProfessionalByUserIdAsync(command.SupervisorUserId.Value, cancellationToken);
                    var family = await _repository.GetFamilyByUserIdAsync(command.SupervisorUserId.Value, cancellationToken);

                    if (supervisor == null && family == null)
                    {
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.SupervisorNotAuthorized,
                            ErrorMessages.SupervisorMustBeProfessionalOrFamily);
                    }
                    supervisorUserId = command.SupervisorUserId;
                    break;

                case LoginMethodStandard:
                    // Generar contrasena temporal para que el usuario pueda loguearse
                    temporaryPassword = GenerateTemporaryPassword();
                    var user = await _identityService.FindByIdAsync(command.UserId);
                    if (user == null)
                    {
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.PersonNotFound,
                            ErrorMessages.PersonNotFound);
                    }
                    var (succeeded, errors) = await _identityService.ResetPasswordAsync(user, temporaryPassword);
                    if (!succeeded)
                    {
                        _logger.LogWarning("Error al resetear contrasena para usuario {UserId}: {Errors}",
                            command.UserId, string.Join(", ", errors));
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.ValidationFailed,
                            string.Format(ErrorMessages.UserCreationError, string.Join(", ", errors)));
                    }
                    // Marcar que debe cambiar contrasena en primer login
                    user.MustChangePassword = true;
                    await _identityService.UpdateUserAsync(user);
                    break;

                default:
                    return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                        ErrorCode.LoginMethodNotAllowed,
                        ErrorMessages.LoginMethodNotSupported);
            }

            // Actualizar el metodo de login
            await _repository.UpdatePersonLoginMethodAsync(
                command.UserId,
                command.LoginMethodId,
                pinHash,
                supervisorUserId,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Login method updated for user {UserId} to method {LoginMethodId}",
                command.UserId, command.LoginMethodId);

            return ApiResponse<UpdateLoginMethodResponse>.SuccessResult(
                new UpdateLoginMethodResponse
                {
                    Updated = true,
                    LoginMethodId = loginMethod.Id,
                    LoginMethodName = loginMethod.Name,
                    TemporaryPassword = temporaryPassword
                },
                SuccessMessages.LoginMethodUpdated);
        }

        private static string GenerateTemporaryPassword()
        {
            return $"Temp@{Guid.NewGuid().ToString()[..8]}";
        }
    }
}
