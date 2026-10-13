using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class CancelInvitationCommandHandler : ICommandHandler<CancelInvitationCommand, ApiResponse<InvitationResponse>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelInvitationCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public CancelInvitationCommandHandler(
            IInvitationsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<CancelInvitationCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<InvitationResponse>> HandleAsync(CancelInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var invitation = await _repository.GetByCodeAsync(command.Code, cancellationToken);
                if (invitation == null)
                {
                    return ApiResponse<InvitationResponse>.ErrorResult(
                        ErrorCode.InvitationNotFound,
                        ErrorMessages.InvitationNotFound);
                }

                // Si quien cancela es un profesional, verificar que sea el profesional creador
                if (string.Equals(command.UserRole, "Professional", StringComparison.OrdinalIgnoreCase))
                {
                    if (command.ProfessionalId.HasValue && invitation.CreatedByProfessionalId != command.ProfessionalId.Value)
                    {
                        return ApiResponse<InvitationResponse>.Forbidden();
                    }
                }

                if (invitation.IsUsed)
                {
                    return ApiResponse<InvitationResponse>.ErrorResult(
                        ErrorCode.InvalidOperation,
                        "No se puede cancelar una invitación que ya ha sido aceptada.");
                }

                if (!invitation.IsActive)
                {
                    return ApiResponse<InvitationResponse>.ErrorResult(
                        ErrorCode.InvalidOperation,
                        "La invitación ya se encuentra cancelada.");
                }

                invitation.IsActive = false;
                invitation.UpdatedAt = _dateTime.UtcNow;
                invitation.UpdatedBy = command.UserId;

                await _repository.UpdateAsync(invitation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Invitación {Code} cancelada exitosamente por usuario {UserId}", command.Code, command.UserId);

                var response = InvitationResponse.MapToResponse(invitation);
                return ApiResponse<InvitationResponse>.SuccessResult(response, "Invitación cancelada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar la invitación {Code}", command.Code);
                return ApiResponse<InvitationResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Ocurrió un error al cancelar la invitación.");
            }
        }
    }
}
