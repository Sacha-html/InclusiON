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
    public class GetInvitationsQueryHandler : IQueryHandler<GetInvitationsQuery, ApiResponse<PagedResponse<InvitationResponse>>>
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

        public async Task<ApiResponse<PagedResponse<InvitationResponse>>> HandleAsync(GetInvitationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var paged = query.ProfessionalId.HasValue
                    ? await _repository.GetPagedByProfessionalIdAsync(query.ProfessionalId.Value, query.Page, query.PageSize, query.Search, query.Status, cancellationToken)
                    : query.InstitutionIds != null && query.InstitutionIds.Any()
                        ? await _repository.GetPagedByInstitutionIdsAsync(query.InstitutionIds, query.Page, query.PageSize, query.Search, query.Status, cancellationToken)
                        : await _repository.GetPagedAsync(query.Page, query.PageSize, query.Search, query.Status, cancellationToken);

                var response = new PagedResponse<InvitationResponse>
                {
                    Data = paged.Data.Select(InvitationResponse.MapToResponse).ToList(),
                    TotalRecords = paged.TotalRecords,
                    TotalPages = paged.TotalPages,
                    CurrentPage = paged.CurrentPage,
                    PageSize = paged.PageSize,
                    HasNextPage = paged.HasNextPage,
                    HasPreviousPage = paged.HasPreviousPage
                };

                return ApiResponse<PagedResponse<InvitationResponse>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar invitaciones del profesional {ProfessionalId}", query.ProfessionalId);
                return ApiResponse<PagedResponse<InvitationResponse>>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorListInvitations);
            }
        }
    }
}
