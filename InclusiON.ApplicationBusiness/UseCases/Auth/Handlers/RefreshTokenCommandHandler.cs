using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ApiResponse<LoginResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(
            UserManager<User> userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            ILogger<RefreshTokenCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrWhiteSpace(command.RefreshToken))
                {
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.RequiredField,
                        "Refresh token es requerido");
                }

                var storedToken = await _refreshTokensRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

                if (storedToken is null)
                {
                    _logger.LogWarning("Refresh token not found");
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.TokenInvalid,
                        "Token invalido");
                }

                if (!storedToken.IsActive)
                {
                    _logger.LogWarning("Attempted to use revoked refresh token for user {UserId}", storedToken.UserId);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.TokenInvalid,
                        "Token ha sido revocado");
                }

                if (storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Attempted to use expired refresh token for user {UserId}", storedToken.UserId);
                    await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "Token expired", cancellationToken);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.TokenExpired,
                        "Token ha expirado");
                }

                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("User not found for refresh token");
                    await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User not found", cancellationToken);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.UserNotFound,
                        "Usuario no encontrado");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted to refresh token: {UserId}", user.Id);
                    await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User inactive", cancellationToken);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.AccountInactive,
                        "Usuario inactivo");
                }

                var ipAddress = _httpContextService.GetClientIpAddress();
                var userAgent = _httpContextService.GetUserAgent();

                var roles = await _userManager.GetRolesAsync(user);
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

                var newAccessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                // Calculate remaining days from original token expiry
                var remainingDays = (storedToken.ExpiresAt - DateTime.UtcNow).TotalDays;
                var refreshTokenExpiryDays = Math.Max(1, (int)Math.Ceiling(remainingDays));

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = newRefreshToken,
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
                    await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "Replaced by new token", ct);
                    await _refreshTokensRepository.CreateAsync(refreshTokenEntity, ct);
                }, cancellationToken);

                _logger.LogDebug("Successfully refreshed token for user {UserId}", user.Id);

                var response = new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = _jwtTokenService.GetTokenExpiration(newAccessToken),
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

                return ApiResponse<LoginResponse>.SuccessResult(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al refrescar token");
            }
        }
    }
}
