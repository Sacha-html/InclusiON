using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILoginSessionService _loginSessionService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IIdentityService identityService,
            ILoginSessionService loginSessionService,
            ILogger<LoginCommandHandler> logger)
        {
            _identityService = identityService;
            _loginSessionService = loginSessionService;
            _logger = logger;
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
                        ErrorMessages.InvalidCredentials);
                }

                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.AccountInactive,
                        ErrorMessages.AccountInactive);
                }

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
                            ErrorMessages.TwoFactorRequired);
                    }

                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.InvalidCredentials,
                        ErrorMessages.InvalidCredentials);
                }

                var refreshTokenExpiryDays = command.RememberMe ? 30 : 7;

                return await _loginSessionService.CreateLoginSessionAsync(
                    user,
                    refreshTokenExpiryDays,
                    "New login detected - previous sessions invalidated",
                    SuccessMessages.LoginSuccessful,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para: {Email}", command.Email);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorLogin);
            }
        }
    }
}
