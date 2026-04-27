using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ApiResponse<ChangePasswordResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IIdentityService identityService,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<ApiResponse<ChangePasswordResponse>> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Validar que las contraseñs nuevas coincidan
            if (command.NewPassword != command.ConfirmNewPassword)
            {
                return ApiResponse<ChangePasswordResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ErrorMessages.NewPasswordsMismatch);
            }

            var user = await _identityService.FindByIdAsync(command.UserId);

            if (user is null)
            {
                return ApiResponse<ChangePasswordResponse>.ErrorResult(
                    ErrorCode.UserNotFound,
                    ErrorMessages.UserNotFound);
            }

            // Cambiar contraseña via Identity
            var (succeeded, errors) = await _identityService.ChangePasswordAsync(
                user,
                command.CurrentPassword,
                command.NewPassword);

            if (!succeeded)
            {
                var errorList = errors.ToList();

                // Si el error es por contraseña actual incorrecta
                if (errorList.Any(e => e.Contains("Incorrect password", StringComparison.OrdinalIgnoreCase)))
                {
                    return ApiResponse<ChangePasswordResponse>.ErrorResult(
                        ErrorCode.InvalidCredentials,
                        ErrorMessages.CurrentPasswordIncorrect);
                }

                return ApiResponse<ChangePasswordResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    string.Format(ErrorMessages.ChangePasswordFailed, string.Join(", ", errorList)));
            }

            // Desactivar flag de cambio obligatorio
            user.MustChangePassword = false;
            await _identityService.UpdateUserAsync(user);

            _logger.LogInformation("contraseña cambiada exitosamente para usuario: {UserId}", command.UserId);

            var response = new ChangePasswordResponse { Success = true };
            return ApiResponse<ChangePasswordResponse>.SuccessResult(response, SuccessMessages.PasswordChanged);
        }
    }
}
