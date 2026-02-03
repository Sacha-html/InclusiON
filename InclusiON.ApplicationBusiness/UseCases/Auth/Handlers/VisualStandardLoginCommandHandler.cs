using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    /// Handler para login visual estandar.
    /// La persona con discapacidad se identifica por nombre y luego ingresa su contrasena.
    /// </summary>
    public class VisualStandardLoginCommandHandler : ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<VisualStandardLoginCommandHandler> _logger;
        private readonly DbContext _context;

        private const int MaxFailedAttempts = 5;

        public VisualStandardLoginCommandHandler(
            IVisualLoginRepository repository,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            ILogger<VisualStandardLoginCommandHandler> logger,
            DbContext context)
        {
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _logger = logger;
            _context = context;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            VisualStandardLoginCommand command,
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

                // Verificar contrasena usando SignInManager
                var signInResult = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    command.Password,
                    lockoutOnFailure: true);

                if (!signInResult.Succeeded)
                {
                    var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                    var remaining = MaxFailedAttempts - failedCount;

                    if (signInResult.IsLockedOut)
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

                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            RemainingAttempts = remaining > 0 ? remaining : 0,
                            ErrorMessage = "Contrasena incorrecta"
                        });
                }

                // Login exitoso
                return await GenerateLoginResponseAsync(user, person, command.DeviceId, command.RememberDevice, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login visual estandar para usuario: {UserId}", command.UserId);
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

            var roles = await _userManager.GetRolesAsync(user);
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
                ExpiresAt = DateTime.UtcNow.AddDays(rememberDevice ? 30 : 1),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            // Execute transactional operations
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Nuevo login visual estandar");

                    user.LastLoginDate = DateTime.UtcNow;
                    user.LastLoginIpAddress = ipAddress;
                    user.LastLoginUserAgent = userAgent;
                    await _userManager.UpdateAsync(user);

                    await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

                    if (rememberDevice && !string.IsNullOrEmpty(deviceId))
                    {
                        var device = new TrustedDevice
                        {
                            UserId = user.Id,
                            DeviceId = deviceId,
                            DeviceName = "Dispositivo registrado via login estandar",
                            Browser = _httpContextService.ParseBrowserFromUserAgent(userAgent),
                            RegisteredAt = DateTime.UtcNow,
                            ExpiresAt = DateTime.UtcNow.AddDays(90),
                            IsActive = true
                        };
                        await _repository.RegisterTrustedDeviceAsync(device, cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

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
