using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Telemetry;
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
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public RefreshTokenCommandHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokensRepository,
            ILoginSessionService loginSessionService,
            ITelemetryService telemetryService,
            ILogger<RefreshTokenCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _identityService = identityService;
            _refreshTokensRepository = refreshTokensRepository;
            _loginSessionService = loginSessionService;
            _telemetryService = telemetryService;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                _telemetryService.RecordLogin("invalid_token", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.RequiredField,
                    ErrorMessages.RefreshTokenRequired);
            }

            var storedToken = await _refreshTokensRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

            if (storedToken is null)
            {
                _logger.LogWarning("Refresh token not found");
                _telemetryService.RecordLogin("invalid_token", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenInvalid,
                    ErrorMessages.TokenInvalid);
            }

            if (!storedToken.IsActive)
            {
                _logger.LogWarning("Attempted to use revoked refresh token for user {UserId}", storedToken.UserId);
                _telemetryService.RecordLogin("revoked_token", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenInvalid,
                    ErrorMessages.TokenRevoked);
            }

            if (storedToken.ExpiresAt < _dateTime.UtcNow)
            {
                _logger.LogWarning("Attempted to use expired refresh token for user {UserId}", storedToken.UserId);
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "Token expired", cancellationToken);
                _telemetryService.RecordLogin("expired_token", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.TokenExpired,
                    ErrorMessages.TokenExpired);
            }

            var user = await _identityService.FindByIdAsync(storedToken.UserId);

            if (user is null)
            {
                _logger.LogWarning("User not found for refresh token");
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User not found", cancellationToken);
                _telemetryService.RecordLogin("user_not_found", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted to refresh token: {UserId}", user.Id);
                await _refreshTokensRepository.RevokeAsync(command.RefreshToken, "User inactive", cancellationToken);
                _telemetryService.RecordLogin("inactive", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.AccountInactive,
                    ErrorMessages.UserInactive);
            }

            var remainingDays = (storedToken.ExpiresAt - _dateTime.UtcNow).TotalDays;
            var refreshTokenExpiryDays = Math.Max(1, (int)Math.Ceiling(remainingDays));

            _logger.LogDebug("Successfully refreshed token for user {UserId}", user.Id);

            var result = await _loginSessionService.CreateLoginSessionAsync(
                user,
                refreshTokenExpiryDays,
                "Replaced by new token",
                SuccessMessages.TokenRefreshed,
                cancellationToken);

            if (result.Success)
            {
                _telemetryService.RecordLogin("token_refreshed", null);
            }

            return result;
        }
    }
}
