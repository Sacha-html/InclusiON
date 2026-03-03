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
    public class PinLoginCommandHandler : ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoginSessionService _loginSessionService;

        private const int MaxFailedAttempts = 5;

        public PinLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            ILoginSessionService loginSessionService)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _loginSessionService = loginSessionService;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            PinLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            if (string.IsNullOrEmpty(person.PinCodeHash))
            {
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.PinNotConfigured,
                    ErrorMessages.PinNotConfigured);
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
                        ErrorMessage = ErrorMessages.PinIncorrect
                    });
            }

            await _identityService.ResetAccessFailedCountAsync(user);

            var refreshTokenExpiryDays = command.RememberDevice ? 30 : 1;

            return await _loginSessionService.CreateVisualLoginSessionAsync(
                user,
                person,
                refreshTokenExpiryDays,
                command.DeviceId,
                command.RememberDevice,
                "Nuevo login con PIN",
                SuccessMessages.VisualLoginSuccessful,
                cancellationToken);
        }
    }
}
