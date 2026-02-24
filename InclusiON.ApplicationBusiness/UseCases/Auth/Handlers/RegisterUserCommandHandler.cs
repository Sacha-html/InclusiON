using Microsoft.AspNetCore.Identity;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, ApiResponse<UserResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<UserResponse>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (command.Password != command.ConfirmPassword)
                {
                    return ApiResponse<UserResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "Las contrasenas no coinciden");
                }

                var existingUser = await _userManager.FindByEmailAsync(command.Email);

                if (existingUser != null)
                {
                    return ApiResponse<UserResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        "El email ya esta registrado");
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

                // Execute transactional operations
                await _unitOfWork.ExecuteInTransactionAsync(async _ =>
                {
                    var result = await _userManager.CreateAsync(user, command.Password);

                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(p => p.Description).ToList();
                        throw new InvalidOperationException(string.Join(", ", errors));
                    }

                    await _userManager.AddToRoleAsync(user, command.Role.ToString());
                }, cancellationToken);

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
            catch (InvalidOperationException ex)
            {
                return ApiResponse<UserResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ex.Message);
            }
            catch (Exception)
            {
                return ApiResponse<UserResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al registrar usuario");
            }
        }
    }
}
