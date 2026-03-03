using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, ApiResponse<UserResponse>>
    {
        private readonly IIdentityService _identityService;

        public RegisterUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
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

            var existingUser = await _identityService.FindByEmailAsync(command.Email);

            if (existingUser != null)
            {
                return ApiResponse<UserResponse>.Conflict(
                    ErrorCode.EmailAlreadyExists,
                    ErrorMessages.EmailAlreadyRegistered);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Surname = command.Surname?.Trim(),
                Email = command.Email.ToLower().Trim(),
                UserName = command.Email.ToLower().Trim(),
                PhoneNumber = command.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow,
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
