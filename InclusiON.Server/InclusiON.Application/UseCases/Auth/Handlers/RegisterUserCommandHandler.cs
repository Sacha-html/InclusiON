using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, ApiResponse<UserResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IDateTimeProvider _dateTime;

        public RegisterUserCommandHandler(
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            IDateTimeProvider dateTime)
        {
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<UserResponse>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (command.Password != command.ConfirmPassword)
            {
                return ApiResponse<UserResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ErrorMessages.PasswordsMismatch);
            }

            var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;

            var emailCheck = await _uniquenessValidation.CheckEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
            if (!emailCheck.IsAvailable)
            {
                return ApiResponse<UserResponse>.Conflict(
                    ErrorCode.EmailAlreadyExists,
                    emailCheck.Message ?? ErrorMessages.EmailAlreadyRegistered);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Surname = command.Surname?.Trim(),
                Email = normalizedEmail,
                UserName = normalizedEmail,
                PhoneNumber = command.PhoneNumber?.Trim(),
                CreatedAt = _dateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var (succeeded, errors) = await _identityService.CreateUserAsync(user, command.Password);

            if (!succeeded)
            {
                return ApiResponse<UserResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    string.Join(", ", errors));
            }

            await _identityService.AddToRoleAsync(user, command.Role.ToString());

            return ApiResponse<UserResponse>.SuccessResult(new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            }, SuccessMessages.UserRegistered);
        }
    }
}
