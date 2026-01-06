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
    /// Handler para login automatico desde dispositivo confiable.
    /// </summary>
    public class TrustedDeviceLoginCommandHandler : ICommandHandler<TrustedDeviceLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TrustedDeviceLoginCommandHandler> _logger;

        public TrustedDeviceLoginCommandHandler(
            IVisualLoginRepository repository,
            UserManager<User> userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TrustedDeviceLoginCommandHandler> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            TrustedDeviceLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var trustedDevice = await _repository.GetTrustedDeviceAsync(command.UserId, command.DeviceId, cancellationToken);

                if (trustedDevice == null)
                {
                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Dispositivo no reconocido o expirado"
                        });
                }

                var user = trustedDevice.User;

                if (!user.IsActive)
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult("Usuario inactivo");
                }

                await _repository.UpdateDeviceLastUsedAsync(trustedDevice.Id, cancellationToken);

                var person = await _repository.GetPersonByUserIdAsync(user.Id, cancellationToken);
                if (person != null)
                {
                    return await GeneratePersonLoginResponseAsync(user, person, cancellationToken);
                }

                var professional = await _repository.GetProfessionalByUserIdAsync(user.Id, cancellationToken);
                if (professional != null)
                {
                    return await GenerateProfessionalLoginResponseAsync(user, professional, cancellationToken);
                }

                var family = await _repository.GetFamilyByUserIdAsync(user.Id, cancellationToken);
                if (family != null)
                {
                    return await GenerateFamilyLoginResponseAsync(user, family, cancellationToken);
                }

                return ApiResponse<VisualLoginResponse>.ErrorResult("Perfil de usuario no encontrado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login con dispositivo confiable: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult($"Error al procesar login: {ex.Message}");
            }
        }

        private async Task<ApiResponse<VisualLoginResponse>> GeneratePersonLoginResponseAsync(
            User user,
            PersonWithDisability person,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Login desde dispositivo confiable");

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
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

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
                "Login exitoso desde dispositivo confiable");
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateProfessionalLoginResponseAsync(
            User user,
            Professional professional,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Login desde dispositivo confiable");

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIpAddress = ipAddress;
            user.LastLoginUserAgent = userAgent;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
                Name = $"{professional.FirstName} {professional.LastName}",
                Role = roles.FirstOrDefault() ?? "Professional",
                IsActive = user.IsActive
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            var displayName = $"{professional.FirstName} {professional.LastName}".Trim();

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
                        AvatarColor = "#4CAF50",
                        UserType = "Professional",
                        Roles = roles.ToList()
                    }
                },
                "Login exitoso desde dispositivo confiable");
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateFamilyLoginResponseAsync(
            User user,
            FamilyRepresentative family,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Login desde dispositivo confiable");

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIpAddress = ipAddress;
            user.LastLoginUserAgent = userAgent;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
                Name = $"{family.FirstName} {family.LastName}",
                Role = roles.FirstOrDefault() ?? "Family",
                IsActive = user.IsActive
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            var displayName = $"{family.FirstName} {family.LastName}".Trim();

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
                        AvatarColor = "#9C27B0",
                        UserType = "Family",
                        Roles = roles.ToList()
                    }
                },
                "Login exitoso desde dispositivo confiable");
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
