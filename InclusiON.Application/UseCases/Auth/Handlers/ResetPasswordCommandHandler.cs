using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, ApiResponse<object>>
    {
        private readonly IIdentityService _identityService;
        private readonly IPasswordResetTokensRepository _passwordResetTokensRepository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IIdentityService identityService,
            IPasswordResetTokensRepository passwordResetTokensRepository,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _identityService = identityService;
            _passwordResetTokensRepository = passwordResetTokensRepository;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (command.NewPassword != command.ConfirmNewPassword)
                return ApiResponse<object>.ErrorResult(ErrorCode.ValidationFailed, "Las contraseñas no coinciden.");

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(command.Token))
            ).ToLowerInvariant();

            var resetToken = await _passwordResetTokensRepository.GetValidByHashAsync(tokenHash, cancellationToken);

            if (resetToken is null)
            {
                // No revelar si expiró o nunca existió.
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.PasswordResetTokenInvalid,
                    "El enlace de recuperación no es válido o ya fue utilizado. Solicitá uno nuevo.");
            }

            if (resetToken.ExpiresAt <= _dateTime.UtcNow)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.PasswordResetTokenExpired,
                    "El enlace de recuperación expiró. Solicitá uno nuevo.");
            }

            var user = resetToken.User;

            var (succeeded, errors) = await _identityService.ResetPasswordAsync(user, command.NewPassword);
            if (!succeeded)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    $"La contraseña no cumple los requisitos: {string.Join(", ", errors)}");
            }

            user.MustChangePassword = false;
            await _identityService.UpdateUserAsync(user);

            // Marcar token como usado.
            resetToken.IsUsed = true;
            resetToken.UsedAt = _dateTime.UtcNow;

            // Revocar todas las sesiones activas.
            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                user.Id, RevokeReasons.UserPasswordReset, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Password reset completed for user {UserId} ({Email})", user.Id, user.Email);

            return ApiResponse<object>.SuccessResult(
                new { },
                "Contraseña actualizada correctamente. Ya podés iniciar sesión.");
        }
    }
}
