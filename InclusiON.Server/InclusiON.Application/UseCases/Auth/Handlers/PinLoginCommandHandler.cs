using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Domain.Models;
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
        private readonly IPinHasher _pinHasher;
        private readonly ILoginSessionService _loginSessionService;
        private readonly IPersonsRepository _personsRepository;
        private readonly IRealTimeNotifier _notifier;

        private const int MaxFailedAttempts = 5;

        public PinLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            IPinHasher pinHasher,
            ILoginSessionService loginSessionService,
            IPersonsRepository personsRepository,
            IRealTimeNotifier notifier)
        {
            _repository = repository;
            _identityService = identityService;
            _pinHasher = pinHasher;
            _loginSessionService = loginSessionService;
            _personsRepository = personsRepository;
            _notifier = notifier;
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

            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user == null)
            {
                return ApiResponse<VisualLoginResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

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

            var pinValid = _pinHasher.Verify(person.PinCodeHash, command.Pin, out var needsRehash);
            if (!pinValid)
            {
                await _identityService.AccessFailedAsync(user);
                var failedCount = await _identityService.GetAccessFailedCountAsync(user);
                var remaining = MaxFailedAttempts - failedCount;

                if (await _identityService.IsLockedOutAsync(user))
                {
                    await NotifyAccountLockedAsync(person, cancellationToken);
                }

                return ApiResponse<VisualLoginResponse>.SuccessResult(
                    new VisualLoginResponse
                    {
                        Success = false,
                        RemainingAttempts = remaining > 0 ? remaining : 0,
                        ErrorMessage = ErrorMessages.PinIncorrect
                    });
            }

            await _identityService.ResetAccessFailedCountAsync(user);

            // Migración lazy BCrypt → Argon2id: rehashear en background sin bloquear el login
            if (needsRehash)
            {
                var newHash = _pinHasher.Hash(command.Pin);
                _ = _repository.UpdatePersonLoginMethodAsync(
                    user.Id,
                    person.LoginMethodId!.Value,
                    newHash,
                    person.SupervisorUserId,
                    CancellationToken.None);
            }

            var refreshTokenExpiryDays = command.RememberDevice ? 30 : 1;

            return await _loginSessionService.CreateVisualLoginSessionAsync(
                user,
                person,
                refreshTokenExpiryDays,
                command.DeviceId,
                command.RememberDevice,
                Constants.RevokeReasons.NewLogin,
                SuccessMessages.VisualLoginSuccessful,
                cancellationToken);
        }

        /// <summary>
        /// Avisa en tiempo real a los tutores/familiares y al profesional supervisor
        /// de que la cuenta del alumno quedó bloqueada por reintentos fallidos de PIN.
        /// </summary>
        private async Task NotifyAccountLockedAsync(PersonWithDisability person, CancellationToken cancellationToken)
        {
            var personName = $"{person.FirstName} {person.LastName}";
            var message = $"{personName} superó los intentos de PIN permitidos y su cuenta quedó bloqueada temporalmente.";
            var tasks = new List<Task>();

            var representatives = await _personsRepository.GetActiveRepresentativesAsync(person.Id, cancellationToken);
            foreach (var rep in representatives)
            {
                if (rep.Representative != null && rep.Representative.UserId != Guid.Empty)
                {
                    tasks.Add(_notifier.NotifyUserAsync(
                        rep.Representative.UserId.ToString(),
                        "🔒 Cuenta bloqueada",
                        message,
                        actionUrl: "/#/family/dashboard",
                        cancellationToken: cancellationToken));
                }
            }

            var supervisors = await _personsRepository.GetSupervisingProfessionalsAsync(person.Id, cancellationToken);
            var notifiedProfUserIds = new HashSet<string>();
            foreach (var prof in supervisors)
            {
                var profUserId = prof.UserId.ToString();
                if (notifiedProfUserIds.Add(profUserId))
                {
                    tasks.Add(_notifier.NotifyUserAsync(
                        profUserId,
                        "🔒 Cuenta bloqueada",
                        message,
                        actionUrl: $"/#/pro/persons/{person.Id}",
                        cancellationToken: cancellationToken));
                }
            }

            if (person.SupervisorUserId.HasValue)
            {
                var specificSupervisorUserIdStr = person.SupervisorUserId.Value.ToString();
                if (notifiedProfUserIds.Add(specificSupervisorUserIdStr))
                {
                    tasks.Add(_notifier.NotifyUserAsync(
                        specificSupervisorUserIdStr,
                        "🔒 Cuenta bloqueada",
                        message,
                        actionUrl: $"/#/pro/persons/{person.Id}",
                        cancellationToken: cancellationToken));
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
    }
}
