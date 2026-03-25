using Microsoft.Extensions.Logging;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminResetPasswordCommandHandler : ICommandHandler<AdminResetPasswordCommand, ApiResponse<ResetPasswordResultResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminResetPasswordCommandHandler> _logger;

        public AdminResetPasswordCommandHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokensRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<AdminResetPasswordCommandHandler> logger)
        {
            _identityService = identityService;
            _refreshTokensRepository = refreshTokensRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ResetPasswordResultResponse>> HandleAsync(
            AdminResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user is null)
                return ApiResponse<ResetPasswordResultResponse>.NotFound("Usuario");

            var tempPassword = PasswordGenerator.GenerateTemporary();

            var (succeeded, errors) = await _identityService.ResetPasswordAsync(user, tempPassword);
            if (!succeeded)
            {
                return ApiResponse<ResetPasswordResultResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    $"Error al resetear contraseña: {string.Join(", ", errors)}");
            }

            user.MustChangePassword = true;
            await _identityService.UpdateUserAsync(user);

            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                user.Id, Constants.RevokeReasons.AdminPasswordReset, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Password reset by admin {AdminId} for user {UserId} ({Email})",
                command.RequestedByUserId, user.Id, user.Email);

            // Enviar email con contraseña temporal (si el usuario tiene email)
            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendTemplatedEmailAsync(
                        user.Email,
                        "Tu contraseña ha sido reseteada — InclusiON",
                        "PasswordReset",
                        new Dictionary<string, string?>
                        {
                            { "UserName", user.Name ?? "Usuario" },
                            { "TemporaryPassword", tempPassword },
                            { "Year", DateTime.UtcNow.Year.ToString() }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar email de reset a {Email}", user.Email);
                }
            }

            return ApiResponse<ResetPasswordResultResponse>.SuccessResult(
                new ResetPasswordResultResponse
                {
                    TemporaryPassword = tempPassword,
                    UserEmail = user.Email ?? string.Empty
                },
                "Contraseña reseteada exitosamente.");
        }
    }
}
