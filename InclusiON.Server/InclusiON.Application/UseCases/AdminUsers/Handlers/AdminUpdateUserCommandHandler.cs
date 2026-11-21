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
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly ILogger<AdminUpdateUserCommandHandler> _logger;

        public AdminUpdateUserCommandHandler(
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            ILogger<AdminUpdateUserCommandHandler> logger)
        {
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
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

            var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;

            if (!string.Equals(user.Email?.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailCheck = await _uniquenessValidation.CheckEmailAsync(
                    normalizedEmail, excludeUserId: command.UserId, cancellationToken: cancellationToken);
                if (!emailCheck.IsAvailable)
                {
                    return ApiResponse<object>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        emailCheck.Message ?? "El email ya está en uso por otro usuario.");
                }

                user.Email = normalizedEmail;
                user.UserName = normalizedEmail;
                user.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                user.NormalizedUserName = normalizedEmail.ToUpperInvariant();
            }

            user.Name = command.Name.Trim();
            user.Surname = command.Surname.Trim();

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
