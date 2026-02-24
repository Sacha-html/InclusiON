using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para login con PIN numerico.
    /// </summary>
    public class PinLoginCommandHandler : ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<PinLoginCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private const int MaxFailedAttempts = 5;

        public PinLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            ILogger<PinLoginCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            PinLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult(
                        ErrorCode.UserNotFound,
                        "Usuario no encontrado");
                }

                var user = person.User;

                // Verificar si esta bloqueado
                if (await _identityService.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _identityService.GetLockoutEndDateAsync(user);
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
                    return ApiResponse<VisualLoginResponse>.ErrorResult(
                        ErrorCode.PinNotConfigured,
                        "PIN no configurado para este usuario");
                }

                var pinValid = _passwordHasher.VerifyPassword(person.PinCodeHash, command.Pin);
                if (!pinValid)
                {
                    await _identityService.AccessFailedAsync(user);
                    var failedCount = await _identityService.GetAccessFailedCountAsync(user);
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
                await _identityService.ResetAccessFailedCountAsync(user);
                return await GenerateLoginResponseAsync(user, person, command.DeviceId, command.RememberDevice, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login con PIN para usuario: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al procesar login");
            }
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateLoginResponseAsync(
            User user,
            PersonWithDisability person,
            string? deviceId,
            bool rememberDevice,
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
                ExpiresAt = DateTime.UtcNow.AddDays(rememberDevice ? 30 : 1),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            // Execute transactional operations
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Nuevo login con PIN");

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;
                await _identityService.UpdateUserAsync(user);

                await _refreshTokensRepository.CreateAsync(refreshTokenEntity, ct);

                if (rememberDevice && !string.IsNullOrEmpty(deviceId))
                {
                    var device = new TrustedDevice
                    {
                        UserId = user.Id,
                        DeviceId = deviceId,
                        DeviceName = "Dispositivo registrado via PIN",
                        Browser = _httpContextService.ParseBrowserFromUserAgent(userAgent),
                        RegisteredAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(90),
                        IsActive = true
                    };
                    await _repository.RegisterTrustedDeviceAsync(device, ct);
                }
            }, cancellationToken);

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
    }
}
