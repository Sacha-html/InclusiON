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
    /// Handler para login con PIN numerico.
    /// </summary>
    public class PinLoginCommandHandler : ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PinLoginCommandHandler> _logger;

        private const int MaxFailedAttempts = 5;

        public PinLoginCommandHandler(
            IVisualLoginRepository repository,
            UserManager<User> userManager,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PinLoginCommandHandler> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            PinLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var userId = Guid.Parse(command.UserId.ToString());
                var person = await _repository.GetPersonByUserIdAsync(userId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult("Usuario no encontrado");
                }

                var user = person.User;

                // Verificar si esta bloqueado
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var secondsRemaining = lockoutEnd.HasValue
                        ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds
                        : 0;

                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            IsLocked = true,
                            LockoutSecondsRemaining = secondsRemaining,
                            ErrorMessage = "Cuenta bloqueada por intentos fallidos"
                        });
                }

                // Verificar PIN
                if (string.IsNullOrEmpty(person.PinCodeHash))
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult("PIN no configurado para este usuario");
                }

                var pinValid = _passwordHasher.VerifyPassword(person.PinCodeHash, command.Pin);
                if (!pinValid)
                {
                    await _userManager.AccessFailedAsync(user);
                    var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                    var remaining = MaxFailedAttempts - failedCount;

                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            RemainingAttempts = remaining > 0 ? remaining : 0,
                            ErrorMessage = "PIN incorrecto"
                        });
                }

                // Login exitoso
                await _userManager.ResetAccessFailedCountAsync(user);
                return await GenerateLoginResponseAsync(user, person, command.DeviceId, command.RememberDevice, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login con PIN para usuario: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult($"Error al procesar login: {ex.Message}");
            }
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateLoginResponseAsync(
            User user,
            PersonWithDisability person,
            string? deviceId,
            bool rememberDevice,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Nuevo login con PIN");

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIpAddress = ipAddress;
            user.LastLoginUserAgent = userAgent;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
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
                ExpiresAt = DateTime.UtcNow.AddDays(rememberDevice ? 30 : 1),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            if (rememberDevice && !string.IsNullOrEmpty(deviceId))
            {
                var device = new TrustedDevice
                {
                    UserId = user.Id,
                    DeviceId = deviceId,
                    DeviceName = "Dispositivo registrado via PIN",
                    Browser = ParseBrowserFromUserAgent(userAgent),
                    RegisteredAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(90),
                    IsActive = true
                };
                await _repository.RegisterTrustedDeviceAsync(device, cancellationToken);
            }

            var displayName = $"{person.FirstName} {person.LastName}".Trim();

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
                "Login exitoso");
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

        private static string? ParseBrowserFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return null;
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            return "Other";
        }
    }
}
