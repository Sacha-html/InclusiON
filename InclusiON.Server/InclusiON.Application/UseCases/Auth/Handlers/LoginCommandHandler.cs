using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Telemetry;
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
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IIdentityService identityService,
            ILoginSessionService loginSessionService,
            ITelemetryService telemetryService,
            ILogger<LoginCommandHandler> logger)
        {
            _identityService = identityService;
            _loginSessionService = loginSessionService;
            _telemetryService = telemetryService;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await _identityService.FindByEmailAsync(command.Email.ToLower().Trim());

            if (user is null)
            {
                _telemetryService.RecordLogin("failure", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.InvalidCredentials,
                    ErrorMessages.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                _telemetryService.RecordLogin("inactive", null);
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

                _logger.LogWarning("Login attempt for locked account: {UserId}", user.Id);
                _telemetryService.RecordLogin("locked", null);
                return ApiResponse<LoginResponse>.AccountLocked(minutesRemaining);
            }

            var signInStatus = await _identityService
                .CheckPasswordAsync(user, command.Password, lockoutOnFailure: true);

            if (signInStatus != SignInStatus.Success)
            {
                if (signInStatus == SignInStatus.LockedOut)
                {
                    _telemetryService.RecordLogin("locked", null);
                    return ApiResponse<LoginResponse>.AccountLocked();
                }

                if (signInStatus == SignInStatus.RequiresTwoFactor)
                {
                    _telemetryService.RecordLogin("two_factor_required", null);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.TwoFactorRequired,
                        ErrorMessages.TwoFactorRequired);
                }

                _telemetryService.RecordLogin("failure", null);
                return ApiResponse<LoginResponse>.ErrorResult(
                    ErrorCode.InvalidCredentials,
                    ErrorMessages.InvalidCredentials);
            }

            if (command.AllowedRoles is not null && command.AllowedRoles.Count > 0)
            {
                var userRoles = await _identityService.GetRolesAsync(user);
                if (!userRoles.Any(r => command.AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning(
                        "Login rejected for {Email}: role {Roles} not in allowed roles {AllowedRoles}",
                        command.Email, string.Join(",", userRoles), string.Join(",", command.AllowedRoles));
                    
                    _telemetryService.RecordLogin("role_not_allowed", null);
                    return ApiResponse<LoginResponse>.ErrorResult(
                        ErrorCode.RoleNotAllowedForLogin,
                        "No tienes permisos para acceder desde este portal.");
                }
            }

            var institutionId = user.AdminInstitutions?.FirstOrDefault()?.InstitutionId.ToString();
            var refreshTokenExpiryDays = command.RememberMe ? 30 : 7;

            var result = await _loginSessionService.CreateLoginSessionAsync(
                user,
                refreshTokenExpiryDays,
                Constants.RevokeReasons.NewLogin,
                SuccessMessages.LoginSuccessful,
                cancellationToken);

            if (result.Success)
            {
                _telemetryService.RecordLogin("success", institutionId);
            }
            else
            {
                _telemetryService.RecordLogin("error", institutionId);
            }

            return result;
        }
    }
}
