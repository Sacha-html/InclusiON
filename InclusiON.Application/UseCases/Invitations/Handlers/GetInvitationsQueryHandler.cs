using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class GetInvitationsQueryHandler : IQueryHandler<GetInvitationsQuery, ApiResponse<List<InvitationResponse>>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly ILogger<GetInvitationsQueryHandler> _logger;

        public GetInvitationsQueryHandler(
            IInvitationsRepository repository,
            ILogger<GetInvitationsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<InvitationResponse>>> HandleAsync(GetInvitationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var invitations = query.ProfessionalId.HasValue
                    ? await _repository.GetByProfessionalIdAsync(query.ProfessionalId.Value, cancellationToken)
                    : query.InstitutionIds != null && query.InstitutionIds.Any()
                        ? await _repository.GetByInstitutionIdsAsync(query.InstitutionIds, cancellationToken)
                        : await _repository.GetAllAsync(cancellationToken);

                var response = invitations
                    .Select(InvitationResponse.MapToResponse)
                    .ToList();

                return ApiResponse<List<InvitationResponse>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar invitaciones del profesional {ProfessionalId}", query.ProfessionalId);
                return ApiResponse<List<InvitationResponse>>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorListInvitations);
            }
        }
    }
}
