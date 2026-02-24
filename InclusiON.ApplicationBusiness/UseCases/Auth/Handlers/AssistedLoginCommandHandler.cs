using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para login asistido.
    /// Un profesional o familiar autoriza el acceso de una persona con discapacidad
    /// usando sus credenciales de email y contrasena.
    /// </summary>
    public class AssistedLoginCommandHandler : ICommandHandler<AssistedLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<AssistedLoginCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AssistedLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            ILogger<AssistedLoginCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            AssistedLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 1. Buscar la persona con discapacidad
                var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult(
                        ErrorCode.UserNotFound,
                        "Usuario no encontrado");
                }

                // 2. Buscar al supervisor por email
                var supervisor = await _identityService.FindByEmailAsync(command.SupervisorEmail.ToLower().Trim());

                if (supervisor == null)
                {
                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Credenciales del supervisor invalidas"
                        });
                }

                // 3. Verificar que el supervisor esta autorizado
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
                            ErrorMessage = "No tienes autorizacion para asistir a este usuario"
                        });
                }

                // 4. Verificar credenciales del supervisor
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
                                ErrorMessage = "Cuenta del supervisor bloqueada por intentos fallidos"
                            });
                    }

                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Credenciales del supervisor invalidas"
                        });
                }

                // 5. Login exitoso - generar tokens para la persona con discapacidad
                return await GenerateLoginResponseAsync(
                    person.User,
                    person,
                    supervisor,
                    command.DeviceId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login asistido para usuario: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al procesar login");
            }
        }

        private async Task<bool> IsAuthorizedSupervisorAsync(
            PersonWithDisability person,
            Guid supervisorUserId,
            CancellationToken cancellationToken)
        {
            // 1. Verificar si es el supervisor designado
            if (person.SupervisorUserId.HasValue && person.SupervisorUserId.Value == supervisorUserId)
            {
                return true;
            }

            // 2. Obtener el usuario supervisor una sola vez
            var supervisorUser = await _identityService.FindByIdAsync(supervisorUserId);
            if (supervisorUser == null)
            {
                _logger.LogWarning("Supervisor user not found: {SupervisorUserId}", supervisorUserId);
                return false;
            }

            var roles = await _identityService.GetRolesAsync(supervisorUser);

            // 3. Verificar si es un profesional asignado
            var professional = await _repository.GetProfessionalByUserIdAsync(supervisorUserId, cancellationToken);
            if (professional != null && roles.Contains("Professional"))
            {
                // TODO: En produccion, verificar la relacion ProfessionalPerson
                // para asegurar que el profesional esta asignado a esta persona especifica
                return true;
            }

            // 4. Verificar si es un familiar autorizado
            var family = await _repository.GetFamilyByUserIdAsync(supervisorUserId, cancellationToken);
            if (family != null && roles.Contains("Family"))
            {
                // TODO: En produccion, verificar la relacion PersonRepresentative
                // para asegurar que el familiar esta asociado a esta persona especifica
                return true;
            }

            return false;
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateLoginResponseAsync(
            User user,
            PersonWithDisability person,
            User supervisor,
            string? deviceId,
            CancellationToken cancellationToken)
        {
            var ipAddress = _httpContextService.GetClientIpAddress();
            var userAgent = _httpContextService.GetUserAgent();

            var roles = await _identityService.GetRolesAsync(user);
            var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = $"{person.FirstName} {person.LastName}",
                Role = roles.FirstOrDefault() ?? "Person",
                IsActive = user.IsActive,
                Permissions = permissions
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1), // Sesion asistida de 1 dia
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            // Execute transactional operations
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                // Revocar tokens anteriores de la persona
                await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Nuevo login asistido");

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;
                await _identityService.UpdateUserAsync(user);

                await _refreshTokensRepository.CreateAsync(refreshTokenEntity, ct);
            }, cancellationToken);

            var displayName = $"{person.FirstName} {person.LastName}".Trim();

            _logger.LogInformation(
                "Login asistido exitoso. Persona: {PersonId}, Supervisor: {SupervisorId}",
                user.Id, supervisor.Id);

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtTokenService.GetTokenExpiration(accessToken),
                    User = new VisualLoginUserInfo
                    {
                        Id = user.Id,
                        DisplayName = displayName,
                        Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                        AvatarColor = person.AvatarColor ?? "#2196F3",
                        UserType = "Person",
                        Roles = roles.ToList(),
                        Accessibility = new AccessibilityPreferences
                        {
                            RequiresLargeFont = person.RequiresLargeFont,
                            RequiresHighContrast = person.RequiresHighContrast,
                            VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                            SoundSensitivity = person.SoundSensitivity
                        }
                    }
                },
                "Login asistido exitoso");
        }
    }
}
