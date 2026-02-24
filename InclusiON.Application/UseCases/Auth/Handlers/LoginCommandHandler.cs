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
    public class LoginCommandHandler : ICommandHandler<LoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            IIdentityService identityService,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            ILogger<LoginCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var user = await _identityService.FindByEmailAsync(command.Email.ToLower().Trim());

                if (user is null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.InvalidCredentials,
                        "Email o contrasena invalidos");
                }

                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.AccountInactive,
                        "Usuario inactivo. Contacte a soporte.");
                }

                // Verificar bloqueo antes de intentar login (feedback inmediato al usuario)
                if (await _identityService.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _identityService.GetLockoutEndDateAsync(user);
                    var minutesRemaining = lockoutEnd.HasValue
                        ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                        : 0;

                    _logger.LogWarning("Login attempt for locked account: {Email}", command.Email);
                    return ApiResponse<LoginResponse>.AccountLocked(minutesRemaining);
                }

                var signInStatus = await _identityService
                    .CheckPasswordAsync(user, command.Password, lockoutOnFailure: true);

                if (signInStatus != SignInStatus.Success)
                {
                    if (signInStatus == SignInStatus.LockedOut)
                    {
                        return ApiResponse<LoginResponse>.AccountLocked();
                    }

                    if (signInStatus == SignInStatus.RequiresTwoFactor)
                    {
                        return ApiResponse<LoginResponse>.ErrorResult(
                            ErrorCode.TwoFactorRequired,
                            "Se requiere autenticacion de dos factores");
                    }

                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.InvalidCredentials,
                        "Email o contrasena invalidos");
                }

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

                var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // RememberMe: 30 días (sesión persistente), Sin RememberMe: 7 días (sesión normal)
                var refreshTokenExpiryDays = command.RememberMe ? 30 : 7;

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

                // Execute transactional operations
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var revokedCount = await _refreshTokensRepository
                        .RevokeAllUserTokensAsync(user.Id, "New login detectect - previous sessions was invalidated");

                    if (revokedCount > 0)
                    {
                        _logger.LogDebug("Revoked {RevokedCount} previous tokens for user {UserId}", revokedCount, user.Id);
                    }

                    user.LastLoginDate = DateTime.UtcNow;
                    user.LastLoginIpAddress = ipAddress;
                    user.LastLoginUserAgent = userAgent;

                    await _identityService.UpdateUserAsync(user);
                    await _refreshTokensRepository.CreateAsync(refreshTokenEntity, ct);
                }, cancellationToken);

                var response = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtTokenService.GetTokenExpiration(accessToken),
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

                return ApiResponse<LoginResponse>.SuccessResult(response, "Login succesfull");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para: {Email}", command.Email);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al procesar login");
            }
        }
    }
}
