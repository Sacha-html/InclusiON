using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class ValidateInvitationQueryHandler : IQueryHandler<ValidateInvitationQuery, ApiResponse<InvitationValidationResponse>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly ILogger<ValidateInvitationQueryHandler> _logger;

        public ValidateInvitationQueryHandler(
            IInvitationsRepository repository,
            ILogger<ValidateInvitationQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<InvitationValidationResponse>> HandleAsync(ValidateInvitationQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var invitation = await _repository.GetByCodeAsync(query.Code, cancellationToken);

                if (invitation == null)
                {
                    return ApiResponse<InvitationValidationResponse>.ErrorResult(
                        ErrorCode.InvitationNotFound,
                        ErrorMessages.InvitationNotFound);
                }

                if (invitation.IsUsed)
                {
                    return ApiResponse<InvitationValidationResponse>.ErrorResult(
                        ErrorCode.InvitationAlreadyUsed,
                        ErrorMessages.InvitationAlreadyUsed);
                }

                if (invitation.ExpiresAt < DateTime.UtcNow)
                {
                    return ApiResponse<InvitationValidationResponse>.ErrorResult(
                        ErrorCode.InvitationExpired,
                        ErrorMessages.InvitationExpired);
                }

                var response = new InvitationValidationResponse
                {
                    Code = invitation.Code,
                    Email = invitation.Email,
                    FirstName = invitation.FirstName,
                    LastName = invitation.LastName,
                    Relationship = invitation.Relationship,
                    PersonName = invitation.ForPerson != null
                        ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                        : null
                };

                return ApiResponse<InvitationValidationResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar invitacion {Code}", query.Code);
                return ApiResponse<InvitationValidationResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorValidateInvitation);
            }
        }
    }
}
