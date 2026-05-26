using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminUpdateUserCommandHandler : ICommandHandler<AdminUpdateUserCommand, ApiResponse<object>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AdminUpdateUserCommandHandler> _logger;

        public AdminUpdateUserCommandHandler(
            IIdentityService identityService,
            ILogger<AdminUpdateUserCommandHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            AdminUpdateUserCommand command, CancellationToken cancellationToken)
        {
            if (command.UserId != command.RequestedByUserId)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.Forbidden,
                    "Solo puede editar su propia cuenta.");
            }

            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user is null)
                return ApiResponse<object>.NotFound("Usuario");

            if (!string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _identityService.FindByEmailAsync(command.Email);
                if (existing is not null && existing.Id != command.UserId)
                {
                    return ApiResponse<object>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        "El email ya está en uso por otro usuario.");
                }

                user.Email = command.Email;
                user.UserName = command.Email;
                user.NormalizedEmail = command.Email.ToUpperInvariant();
                user.NormalizedUserName = command.Email.ToUpperInvariant();
            }

            user.Name = command.Name;
            user.Surname = command.Surname;

            var (succeeded, errors) = await _identityService.UpdateUserAsync(user);
            if (!succeeded)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.InternalError,
                    $"Error al actualizar el usuario: {string.Join(", ", errors)}");
            }

            _logger.LogInformation("Admin {UserId} actualizó sus datos de perfil.", command.UserId);

            return ApiResponse<object>.SuccessResult("Datos actualizados exitosamente.");
        }
    }
}
