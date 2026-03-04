using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services
{
    public class LoginSessionService : ILoginSessionService
    {
        private readonly IIdentityService _identityService;
        private readonly TokenServices _tokenServices;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly IVisualLoginRepository _visualLoginRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginSessionService> _logger;

        public LoginSessionService(
            IIdentityService identityService,
            TokenServices tokenServices,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            IVisualLoginRepository visualLoginRepository,
            IUnitOfWork unitOfWork,
            ILogger<LoginSessionService> logger)
        {
            _identityService = identityService;
            _tokenServices = tokenServices;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _visualLoginRepository = visualLoginRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> CreateLoginSessionAsync(
            User user,
            int refreshTokenExpiryDays,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var ipAddress = _httpContextService.GetClientIpAddress();
            var userAgent = _httpContextService.GetUserAgent();

            var roles = await _identityService.GetRolesAsync(user);
            var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name!,
                Role = roles.FirstOrDefault() ?? "Customer",
                IsActive = user.IsActive,
                Permissions = permissions
            };

            var accessToken = _tokenServices.JwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _tokenServices.JwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var revokedCount = await _tokenServices.RefreshTokensRepository
                    .RevokeAllUserTokensAsync(user.Id, revokeReason);

                if (revokedCount > 0)
                {
                    _logger.LogDebug("Revoked {RevokedCount} previous tokens for user {UserId}", revokedCount, user.Id);
                }

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;

                await _identityService.UpdateUserAsync(user);
                await _tokenServices.RefreshTokensRepository.CreateAsync(refreshTokenEntity, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenServices.JwtTokenService.GetTokenExpiration(accessToken),
                User = new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name!,
                    Surname = user.Surname,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "User",
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate
                }
            };

            return ApiResponse<LoginResponse>.SuccessResult(response, successMessage);
        }

        public async Task<ApiResponse<VisualLoginResponse>> CreateVisualLoginSessionAsync(
            User user,
            PersonWithDisability person,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            string successMessage,
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

            var accessToken = _tokenServices.JwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _tokenServices.JwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _tokenServices.RefreshTokensRepository.RevokeAllUserTokensAsync(user.Id, revokeReason);

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;
                await _identityService.UpdateUserAsync(user);

                await _tokenServices.RefreshTokensRepository.CreateAsync(refreshTokenEntity, ct);

                if (rememberDevice && !string.IsNullOrEmpty(deviceId))
                {
                    var device = new TrustedDevice
                    {
                        UserId = user.Id,
                        DeviceId = deviceId,
                        DeviceName = "Dispositivo registrado",
                        Browser = _httpContextService.ParseBrowserFromUserAgent(userAgent),
                        RegisteredAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(90),
                        IsActive = true
                    };
                    await _visualLoginRepository.RegisterTrustedDeviceAsync(device, ct);
                }

                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var displayName = $"{person.FirstName} {person.LastName}".Trim();

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _tokenServices.JwtTokenService.GetTokenExpiration(accessToken),
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
                successMessage);
        }

        public async Task<ApiResponse<VisualLoginResponse>> CreateFamilyLoginSessionAsync(
            User user,
            FamilyRepresentative family,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            string successMessage,
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
                Name = $"{family.FirstName} {family.LastName}",
                Role = roles.FirstOrDefault() ?? "Family",
                IsActive = user.IsActive,
                Permissions = permissions
            };

            var accessToken = _tokenServices.JwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _tokenServices.JwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _tokenServices.RefreshTokensRepository.RevokeAllUserTokensAsync(user.Id, revokeReason);

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;
                await _identityService.UpdateUserAsync(user);

                await _tokenServices.RefreshTokensRepository.CreateAsync(refreshTokenEntity, ct);

                if (rememberDevice && !string.IsNullOrEmpty(deviceId))
                {
                    var device = new TrustedDevice
                    {
                        UserId = user.Id,
                        DeviceId = deviceId,
                        DeviceName = "Dispositivo registrado",
                        Browser = _httpContextService.ParseBrowserFromUserAgent(userAgent),
                        RegisteredAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(90),
                        IsActive = true
                    };
                    await _visualLoginRepository.RegisterTrustedDeviceAsync(device, ct);
                }

                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var displayName = $"{family.FirstName} {family.LastName}".Trim();

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _tokenServices.JwtTokenService.GetTokenExpiration(accessToken),
                    User = new VisualLoginUserInfo
                    {
                        Id = user.Id,
                        DisplayName = displayName,
                        Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                        AvatarColor = "#9C27B0",
                        UserType = "Family",
                        Roles = roles.ToList(),
                        Accessibility = null
                    }
                },
                successMessage);
        }
    }
}
