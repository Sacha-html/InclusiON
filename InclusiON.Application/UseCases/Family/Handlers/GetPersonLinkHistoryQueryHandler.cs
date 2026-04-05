using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetPersonLinkHistoryQueryHandler : IQueryHandler<GetPersonLinkHistoryQuery, ApiResponse<List<PersonRepresentativeHistoryResponse>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetPersonLinkHistoryQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<List<PersonRepresentativeHistoryResponse>>> HandleAsync(GetPersonLinkHistoryQuery query, CancellationToken cancellationToken)
        {
            var history = await _familyRepository.GetPersonRepresentativeHistoryAsync(query.PersonId, cancellationToken);

            var response = history.Select(PersonRepresentativeHistoryResponse.MapToResponse).ToList();

            return ApiResponse<List<PersonRepresentativeHistoryResponse>>.SuccessResult(response);
        }
    }
}
