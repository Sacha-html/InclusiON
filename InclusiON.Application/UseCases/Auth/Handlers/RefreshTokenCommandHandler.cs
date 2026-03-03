using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ApiResponse<LoginResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly ILoginSessionService _loginSessionService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokensRepository,
            ILoginSessionService loginSessionService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _identityService = identityService;
            _refreshTokensRepository = refreshTokensRepository;
            _loginSessionService = loginSessionService;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.RequiredField,
                    ErrorMessages.RefreshTokenRequired);
            }

            var storedToken = await _refreshTokensRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

            if (storedToken is null)
            {
                _logger.LogWarning("Refresh token not found");
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenInvalid,
                    ErrorMessages.TokenInvalid);
            }

            if (!storedToken.IsActive)
            {
                _logger.LogWarning("Attempted to use revoked refresh token for user {UserId}", storedToken.UserId);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenInvalid,
                    ErrorMessages.TokenRevoked);
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Attempted to use expired refresh token for user {UserId}", storedToken.UserId);
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "Token expired", cancellationToken);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenExpired,
                    ErrorMessages.TokenExpired);
            }

            var user = await _identityService.FindByIdAsync(storedToken.UserId);

            if (user is null)
            {
                _logger.LogWarning("User not found for refresh token");
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User not found", cancellationToken);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted to refresh token: {UserId}", user.Id);
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User inactive", cancellationToken);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.AccountInactive,
                    ErrorMessages.UserInactive);
            }

            var remainingDays = (storedToken.ExpiresAt - DateTime.UtcNow).TotalDays;
            var refreshTokenExpiryDays = Math.Max(1, (int)Math.Ceiling(remainingDays));

            _logger.LogDebug("Successfully refreshed token for user {UserId}", user.Id);

            return await _loginSessionService.CreateLoginSessionAsync(
                user,
                refreshTokenExpiryDays,
                "Replaced by new token",
                SuccessMessages.TokenRefreshed,
                cancellationToken);
        }
    }
}
