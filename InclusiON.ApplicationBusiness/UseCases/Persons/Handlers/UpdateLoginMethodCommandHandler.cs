using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.ApplicationBusiness.UseCases.Persons.Handlers
{
    /// <summary>
    /// Handler para actualizar el metodo de login de una persona con discapacidad.
    /// </summary>
    public class UpdateLoginMethodCommandHandler : ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UpdateLoginMethodCommandHandler> _logger;

        // Constantes para IDs de metodos de login
        private const int LoginMethodStandard = 1;
        private const int LoginMethodPin = 2;
        private const int LoginMethodAssisted = 5;

        public UpdateLoginMethodCommandHandler(
            IVisualLoginRepository repository,
            IPasswordHasher passwordHasher,
            ILogger<UpdateLoginMethodCommandHandler> logger)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<ApiResponse<UpdateLoginMethodResponse>> HandleAsync(
            UpdateLoginMethodCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Verificar que la persona existe
                var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);
                if (person == null)
                {
                    return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                        ErrorCode.PersonNotFound,
                        "Persona no encontrada");
                }

                // Verificar que el metodo de login es valido y activo
                var loginMethod = await _repository.GetLoginMethodByIdAsync(command.LoginMethodId, cancellationToken);
                if (loginMethod == null)
                {
                    return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                        ErrorCode.ResourceNotFound,
                        "Metodo de login no encontrado");
                }

                if (!loginMethod.IsActive)
                {
                    return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                        ErrorCode.LoginMethodNotAllowed,
                        "El metodo de login seleccionado no esta disponible");
                }

                // Validaciones segun el metodo de login
                string? pinHash = null;
                Guid? supervisorUserId = null;

                switch (command.LoginMethodId)
                {
                    case LoginMethodPin:
                        if (string.IsNullOrEmpty(command.Pin))
                        {
                            return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                                ErrorCode.RequiredField,
                                "El PIN es requerido para este metodo de login");
                        }
                        if (command.Pin.Length < 4 || command.Pin.Length > 6 || !command.Pin.All(char.IsDigit))
                        {
                            return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                                ErrorCode.InvalidFormat,
                                "El PIN debe tener entre 4 y 6 digitos numericos");
                        }
                        pinHash = _passwordHasher.HashPassword(command.Pin);
                        break;

                    case LoginMethodAssisted:
                        if (!command.SupervisorUserId.HasValue)
                        {
                            return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                                ErrorCode.RequiredField,
                                "Se requiere un supervisor para el login asistido");
                        }
                        // Verificar que el supervisor existe y es profesional o familiar
                        var supervisor = await _repository.GetProfessionalByUserIdAsync(command.SupervisorUserId.Value, cancellationToken);
                        var family = await _repository.GetFamilyByUserIdAsync(command.SupervisorUserId.Value, cancellationToken);

                        if (supervisor == null && family == null)
                        {
                            return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                                ErrorCode.SupervisorNotAuthorized,
                                "El supervisor debe ser un profesional o familiar registrado");
                        }
                        supervisorUserId = command.SupervisorUserId;
                        break;

                    case LoginMethodStandard:
                        // No requiere configuracion adicional, usa la contrasena del usuario
                        break;

                    default:
                        return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                            ErrorCode.LoginMethodNotAllowed,
                            "Metodo de login no soportado");
                }

                // Actualizar el metodo de login
                await _repository.UpdatePersonLoginMethodAsync(
                    command.UserId,
                    command.LoginMethodId,
                    pinHash,
                    supervisorUserId,
                    cancellationToken);

                _logger.LogInformation(
                    "Login method updated for user {UserId} to method {LoginMethodId}",
                    command.UserId, command.LoginMethodId);

                return ApiResponse<UpdateLoginMethodResponse>.SuccessResult(
                    new UpdateLoginMethodResponse
                    {
                        Updated = true,
                        LoginMethodId = loginMethod.Id,
                        LoginMethodName = loginMethod.Name
                    },
                    "Metodo de login actualizado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login method for user: {UserId}", command.UserId);
                return ApiResponse<UpdateLoginMethodResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al actualizar metodo de login");
            }
        }
    }
}
