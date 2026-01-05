using Microsoft.AspNetCore.Identity;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, ApiResponse<UserResponse>>
    {
        private readonly UserManager<User> _userManager;

        public RegisterUserCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<UserResponse>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (command.Password != command.ConfirmPassword)
                {
                    return ApiResponse<UserResponse>.ErrorResult("Password dont match");
                }

                var existingUser = await _userManager.FindByEmailAsync(command.Email);

                if (existingUser != null)
                {
                    return ApiResponse<UserResponse>.ErrorResult("Email already registered");
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

                var result = await _userManager.CreateAsync(user, command.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(p => p.Description).ToList();
                    return ApiResponse<UserResponse>.ErrorResult("Failed to create a new user", errors);
                }

                await _userManager.AddToRoleAsync(user, command.Role.ToString());

                return ApiResponse<UserResponse>.SuccessResult(new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                }, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.ErrorResult($"An error occurred while registering the user {ex.Message}");
            }
        }
    }
}
