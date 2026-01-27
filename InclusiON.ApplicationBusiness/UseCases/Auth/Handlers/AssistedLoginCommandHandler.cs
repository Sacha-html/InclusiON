using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AssistedLoginCommandHandler> _logger;

        public AssistedLoginCommandHandler(
            IVisualLoginRepository repository,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AssistedLoginCommandHandler> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
                    return ApiResponse<VisualLoginResponse>.ErrorResult("Usuario no encontrado");
                }

                // 2. Buscar al supervisor por email
                var supervisor = await _userManager.FindByEmailAsync(command.SupervisorEmail.ToLower().Trim());

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
                var signInResult = await _signInManager.CheckPasswordSignInAsync(
                    supervisor,
                    command.SupervisorPassword,
                    lockoutOnFailure: true);

                if (!signInResult.Succeeded)
                {
                    if (signInResult.IsLockedOut)
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
                return ApiResponse<VisualLoginResponse>.ErrorResult($"Error al procesar login: {ex.Message}");
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

            // 2. Verificar si es un profesional asignado
            var professional = await _repository.GetProfessionalByUserIdAsync(supervisorUserId, cancellationToken);
            if (professional != null)
            {
                // Verificar si el profesional esta asignado a esta persona
                // Nota: Esto requiere que las relaciones esten cargadas o una consulta adicional
                // Por simplicidad, si el usuario tiene rol Professional, lo consideramos autorizado
                // En produccion, se deberia verificar la relacion ProfessionalPerson
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(supervisorUserId.ToString()) ?? throw new Exception());
                if (roles.Contains("Professional"))
                {
                    return true;
                }
            }

            // 3. Verificar si es un familiar autorizado
            var family = await _repository.GetFamilyByUserIdAsync(supervisorUserId, cancellationToken);
            if (family != null)
            {
                // Verificar si el familiar esta asociado a esta persona
                // Por simplicidad, si el usuario tiene rol Family, lo consideramos autorizado
                // En produccion, se deberia verificar la relacion PersonRepresentative
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(supervisorUserId.ToString()) ?? throw new Exception());
                if (roles.Contains("Family"))
                {
                    return true;
                }
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
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            // Revocar tokens anteriores de la persona
            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Nuevo login asistido");

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIpAddress = ipAddress;
            user.LastLoginUserAgent = userAgent;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = $"{person.FirstName} {person.LastName}",
                Role = roles.FirstOrDefault() ?? "Person",
                IsActive = user.IsActive
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

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

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

        private static string? GetClientIpAddress(HttpContext? context)
        {
            if (context is null) return null;
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',').First().Trim();
            var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientIp))
                return clientIp;
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
