using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Auth.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para identificar un usuario antes del login.
    /// Busca por nombre, username o email segun el tipo de usuario.
    /// </summary>
    public class IdentifyUserQueryHandler : IQueryHandler<IdentifyUserQuery, ApiResponse<IdentifyUserResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly ILogger<IdentifyUserQueryHandler> _logger;

        public IdentifyUserQueryHandler(
            IVisualLoginRepository repository,
            ILogger<IdentifyUserQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IdentifyUserResponse>> HandleAsync(
            IdentifyUserQuery query,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var identifier = query.Identifier.Trim();

                // Buscar segun tipo de usuario
                switch (query.UserType?.ToUpper())
                {
                    case "PERSON":
                        return await FindPersonAsync(identifier, query.DeviceId, cancellationToken);

                    case "PROFESSIONAL":
                        return await FindProfessionalAsync(identifier, query.DeviceId, cancellationToken);

                    case "FAMILY":
                        return await FindFamilyAsync(identifier, query.DeviceId, cancellationToken);

                    default:
                        // Si no se especifica tipo, buscar en todos
                        var personResult = await FindPersonAsync(identifier, query.DeviceId, cancellationToken);
                        if (personResult.Data?.UserFound == true)
                            return personResult;

                        var professionalResult = await FindProfessionalAsync(identifier, query.DeviceId, cancellationToken);
                        if (professionalResult.Data?.UserFound == true)
                            return professionalResult;

                        var familyResult = await FindFamilyAsync(identifier, query.DeviceId, cancellationToken);
                        if (familyResult.Data?.UserFound == true)
                            return familyResult;

                        return ApiResponse<IdentifyUserResponse>.SuccessResult(
                            new IdentifyUserResponse
                            {
                                UserFound = false,
                                ErrorMessage = "Usuario no encontrado"
                            });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al identificar usuario: {Identifier}", query.Identifier);
                return ApiResponse<IdentifyUserResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al identificar usuario");
            }
        }

        private async Task<ApiResponse<IdentifyUserResponse>> FindPersonAsync(
            string identifier,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var person = await _repository.FindPersonByIdentifierAsync(identifier, cancellationToken);

            if (person == null)
            {
                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse { UserFound = false });
            }

            var isTrusted = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                isTrusted = await _repository.IsTrustedDeviceAsync(person.UserId, deviceId, cancellationToken);
            }

            var displayName = $"{person.FirstName} {person.LastName}".Trim();
            var loginMethod = person.LoginMethod;

            // Verificar si el metodo de login esta deprecado
            if (loginMethod != null && !loginMethod.IsActive)
            {
                _logger.LogWarning(
                    "Usuario {UserId} tiene metodo de login deprecado: {LoginMethod}",
                    person.UserId, loginMethod.Code);

                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse
                    {
                        UserFound = true,
                        UserId = person.UserId,
                        DisplayName = displayName,
                        Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                        AvatarColor = person.AvatarColor ?? "#2196F3",
                        LoginMethodCode = "DEPRECATED",
                        LoginMethodName = "Metodo no disponible",
                        IsTrustedDevice = false,
                        RequiresSupervision = false,
                        UserType = "Person",
                        ErrorMessage = "Tu metodo de acceso necesita actualizarse. Por favor, contacta a un administrador o familiar."
                    },
                    "Metodo de login deprecado");
            }

            // Normalizar codigo SUPERVISED a ASSISTED para retrocompatibilidad
            var loginMethodCode = loginMethod?.Code ?? "STANDARD";
            if (loginMethodCode == "SUPERVISED")
            {
                loginMethodCode = "ASSISTED";
            }

            return ApiResponse<IdentifyUserResponse>.SuccessResult(
                new IdentifyUserResponse
                {
                    UserFound = true,
                    UserId = person.UserId,
                    DisplayName = displayName,
                    Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                    AvatarColor = person.AvatarColor ?? "#2196F3",
                    LoginMethodCode = loginMethodCode,
                    LoginMethodName = loginMethod?.Name ?? "Contrasena",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = loginMethod?.RequiresSupervisor ?? false,
                    UserType = "Person"
                },
                "Usuario identificado correctamente");
        }

        private async Task<ApiResponse<IdentifyUserResponse>> FindProfessionalAsync(
            string identifier,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var professional = await _repository.FindProfessionalByIdentifierAsync(identifier, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse { UserFound = false });
            }

            var isTrusted = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                isTrusted = await _repository.IsTrustedDeviceAsync(professional.UserId, deviceId, cancellationToken);
            }

            var displayName = $"{professional.FirstName} {professional.LastName}".Trim();

            return ApiResponse<IdentifyUserResponse>.SuccessResult(
                new IdentifyUserResponse
                {
                    UserFound = true,
                    UserId = professional.UserId,
                    DisplayName = displayName,
                    Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                    AvatarColor = "#4CAF50",
                    LoginMethodCode = "STANDARD",
                    LoginMethodName = "Contrasena",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = false,
                    UserType = "Professional"
                },
                "Usuario identificado correctamente");
        }

        private async Task<ApiResponse<IdentifyUserResponse>> FindFamilyAsync(
            string identifier,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var family = await _repository.FindFamilyByIdentifierAsync(identifier, cancellationToken);

            if (family == null)
            {
                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse { UserFound = false });
            }

            var isTrusted = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                isTrusted = await _repository.IsTrustedDeviceAsync(family.UserId, deviceId, cancellationToken);
            }

            var displayName = $"{family.FirstName} {family.LastName}".Trim();

            return ApiResponse<IdentifyUserResponse>.SuccessResult(
                new IdentifyUserResponse
                {
                    UserFound = true,
                    UserId = family.UserId,
                    DisplayName = displayName,
                    Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                    AvatarColor = "#9C27B0",
                    LoginMethodCode = "STANDARD",
                    LoginMethodName = "Contrasena",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = false,
                    UserType = "Family"
                },
                "Usuario identificado correctamente");
        }
    }
}
