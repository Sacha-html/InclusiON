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
    public class VisualStandardLoginCommandHandler : ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly ILoginSessionService _loginSessionService;
        private readonly ILogger<VisualStandardLoginCommandHandler> _logger;

        private const int MaxFailedAttempts = 5;

        public VisualStandardLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            ILoginSessionService loginSessionService,
            ILogger<VisualStandardLoginCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _loginSessionService = loginSessionService;
            _logger = logger;
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
                        ErrorMessages.UserNotFound);
                }

                var user = person.User;

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
                            ErrorMessage = ErrorMessages.AccountLocked
                        });
                }

                var signInStatus = await _identityService.CheckPasswordAsync(
                    user,
                    command.Password,
                    lockoutOnFailure: true);

                if (signInStatus != SignInStatus.Success)
                {
                    var failedCount = await _identityService.GetAccessFailedCountAsync(user);
                    var remaining = MaxFailedAttempts - failedCount;

                    if (signInStatus == SignInStatus.LockedOut)
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
                                ErrorMessage = ErrorMessages.AccountLocked
                            });
                    }

                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            RemainingAttempts = remaining > 0 ? remaining : 0,
                            ErrorMessage = ErrorMessages.PasswordIncorrect
                        });
                }

                var refreshTokenExpiryDays = command.RememberDevice ? 30 : 1;

                return await _loginSessionService.CreateVisualLoginSessionAsync(
                    user,
                    person,
                    refreshTokenExpiryDays,
                    command.DeviceId,
                    command.RememberDevice,
                    "Nuevo login visual estandar",
                    SuccessMessages.VisualLoginSuccessful,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login visual estandar para usuario: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorLogin);
            }
        }
    }
}
