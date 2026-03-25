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
    public class FamilyLoginCommandHandler : ICommandHandler<FamilyLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly ILoginSessionService _loginSessionService;

        private const int MaxFailedAttempts = 5;

        public FamilyLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            ILoginSessionService loginSessionService)
        {
            _repository = repository;
            _identityService = identityService;
            _loginSessionService = loginSessionService;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            FamilyLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var family = await _repository.GetFamilyByUserIdAsync(command.UserId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

            var user = family.User;

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

            return await _loginSessionService.CreateFamilyLoginSessionAsync(
                user,
                family,
                refreshTokenExpiryDays,
                command.DeviceId,
                command.RememberDevice,
                Constants.RevokeReasons.NewLogin,
                SuccessMessages.FamilyLoginSuccessful,
                cancellationToken);
        }
    }
}
