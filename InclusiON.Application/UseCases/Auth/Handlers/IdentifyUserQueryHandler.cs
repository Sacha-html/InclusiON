using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Constants;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para identificar un usuario antes del login.
    /// Busca por nombre, username o email segun el tipo de usuario.
    /// </summary>
    public class IdentifyUserQueryHandler : IQueryHandler<IdentifyUserQuery, ApiResponse<IdentifyUserResponse>>
    {
        private const int MinIdentifierLength = 3;
        private const int MaxMatchesShown = 5;

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

            var identifier = query.Identifier.Trim();

            if (identifier.Length < MinIdentifierLength)
            {
                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse
                    {
                        UserFound = false,
                        ErrorMessage = $"Escribe al menos {MinIdentifierLength} letras."
                    });
            }

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
                            ErrorMessage = ErrorMessages.UserNotFound
                        });
            }
        }

        private async Task<ApiResponse<IdentifyUserResponse>> FindPersonAsync(
            string identifier,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var persons = await _repository.FindPersonsByIdentifierAsync(identifier, MaxMatchesShown, cancellationToken);

            if (persons.Count == 0)
            {
                return ApiResponse<IdentifyUserResponse>.SuccessResult(
                    new IdentifyUserResponse { UserFound = false, ErrorMessage = ErrorMessages.UserNotFound });
            }

            if (persons.Count == 1)
            {
                return await BuildSinglePersonResponseAsync(persons[0], deviceId, cancellationToken);
            }

            // Multi-match: devolvemos la lista para que el usuario elija visualmente.
            // No exponemos apellido completo (privacidad), solo inicial.
            var matches = persons.Select(p =>
            {
                var loginMethod = p.LoginMethod;
                var isDeprecated = loginMethod != null && !loginMethod.IsActive;
                return new UserMatchSummary
                {
                    UserId = p.UserId,
                    DisplayName = p.FirstName,
                    Initial = p.FirstName.Length > 0 ? p.FirstName[0].ToString().ToUpper() : "?",
                    LastNameInitial = p.LastName.Length > 0 ? p.LastName[0].ToString().ToUpper() : null,
                    AvatarColor = p.AvatarColor ?? AvatarColors.DefaultPerson,
                    LoginMethodCode = isDeprecated ? "DEPRECATED" : (loginMethod?.Code ?? "STANDARD"),
                    LoginMethodName = isDeprecated ? ErrorMessages.MethodNotAvailable : (loginMethod?.Name ?? "Contraseña"),
                    RequiresSupervision = loginMethod?.RequiresSupervisor ?? false,
                    IsTrustedDevice = false
                };
            }).ToList();

            return ApiResponse<IdentifyUserResponse>.SuccessResult(
                new IdentifyUserResponse
                {
                    UserFound = true,
                    RequiresSelection = true,
                    Matches = matches,
                    UserType = "Person"
                },
                SuccessMessages.UserIdentified);
        }

        private async Task<ApiResponse<IdentifyUserResponse>> BuildSinglePersonResponseAsync(
            PersonWithDisability person,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var isTrusted = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                isTrusted = await _repository.IsTrustedDeviceAsync(person.UserId, deviceId, cancellationToken);
            }

            var displayName = $"{person.FirstName} {person.LastName}".Trim();
            var loginMethod = person.LoginMethod;

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
                        AvatarColor = person.AvatarColor ?? AvatarColors.DefaultPerson,
                        LoginMethodCode = "DEPRECATED",
                        LoginMethodName = ErrorMessages.MethodNotAvailable,
                        IsTrustedDevice = false,
                        RequiresSupervision = false,
                        UserType = "Person",
                        ErrorMessage = ErrorMessages.LoginMethodDeprecated
                    },
                    ErrorMessages.LoginMethodNotAvailable);
            }

            var loginMethodCode = loginMethod?.Code ?? "STANDARD";

            return ApiResponse<IdentifyUserResponse>.SuccessResult(
                new IdentifyUserResponse
                {
                    UserFound = true,
                    UserId = person.UserId,
                    DisplayName = displayName,
                    Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                    AvatarColor = person.AvatarColor ?? AvatarColors.DefaultPerson,
                    LoginMethodCode = loginMethodCode,
                    LoginMethodName = loginMethod?.Name ?? "Contraseña",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = loginMethod?.RequiresSupervisor ?? false,
                    UserType = "Person"
                },
                SuccessMessages.UserIdentified);
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
                    AvatarColor = AvatarColors.DefaultProfessional,
                    LoginMethodCode = "STANDARD",
                    LoginMethodName = "Contraseña",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = false,
                    UserType = "Professional"
                },
                SuccessMessages.UserIdentified);
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
                    AvatarColor = AvatarColors.DefaultFamily,
                    LoginMethodCode = "STANDARD",
                    LoginMethodName = "Contraseña",
                    IsTrustedDevice = isTrusted,
                    RequiresSupervision = false,
                    UserType = "Family"
                },
                SuccessMessages.UserIdentified);
        }
    }
}
