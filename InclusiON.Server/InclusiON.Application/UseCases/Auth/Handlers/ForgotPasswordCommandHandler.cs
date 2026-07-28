using System.Security.Cryptography;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ApiResponse<object>>
    {
        private readonly IIdentityService _identityService;
        private readonly IPasswordResetTokensRepository _passwordResetTokensRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;
        private readonly IPasswordResetConfig _config;

        public ForgotPasswordCommandHandler(
            IIdentityService identityService,
            IPasswordResetTokensRepository passwordResetTokensRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            ILogger<ForgotPasswordCommandHandler> logger,
            IPasswordResetConfig config)
        {
            _identityService = identityService;
            _passwordResetTokensRepository = passwordResetTokensRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _logger = logger;
            _config = config;
        }

        public async Task<ApiResponse<object>> HandleAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            // Respuesta genérica siempre — evita enumeración de usuarios.
            const string genericMessage = "Si el email está registrado en el sistema, recibirás un enlace para restablecer tu contraseña en los próximos minutos.";

            var user = await _identityService.FindByEmailAsync(command.Email);
            if (user is null || !user.IsActive)
            {
                // No revelar que el usuario no existe.
                _logger.LogDebug("Forgot password requested for unknown/inactive email {Email}", command.Email);
                return ApiResponse<object>.SuccessResult(new { }, genericMessage);
            }

            // Invalidar tokens previos para este usuario.
            await _passwordResetTokensRepository.InvalidatePreviousTokensAsync(user.Id, cancellationToken);

            // Generar token seguro: 32 bytes aleatorios → hex string (64 chars).
            var plainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            var tokenHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken))).ToLowerInvariant();

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = _dateTime.UtcNow,
                ExpiresAt = _dateTime.UtcNow.AddMinutes(_config.TokenExpiryMinutes)
            };

            await _passwordResetTokensRepository.CreateAsync(resetToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset token generated for user {UserId}", user.Id);

            if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(command.BaseUrl))
            {
                try
                {
                    var resetUrl = $"{command.BaseUrl.TrimEnd('/')}/#/reset-password?token={plainToken}";

                    await _emailService.SendTemplatedEmailAsync(
                        user.Email,
                        "Recuperar acceso — InclusiON",
                        "ForgotPassword",
                        new Dictionary<string, string?>
                        {
                            { "UserName", user.Name ?? "Usuario" },
                            { "ResetUrl", resetUrl },
                            { "ExpiryMinutes", _config.TokenExpiryMinutes.ToString() },
                            { "Year", _dateTime.UtcNow.Year.ToString() }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar email de recuperación a {Email}", user.Email);
                }
            }

            return ApiResponse<object>.SuccessResult(new { }, genericMessage);
        }
    }
}
