using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyLinkHistoryQueryHandler : IQueryHandler<GetFamilyLinkHistoryQuery, ApiResponse<List<PersonRepresentativeHistoryResponse>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetFamilyLinkHistoryQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<List<PersonRepresentativeHistoryResponse>>> HandleAsync(GetFamilyLinkHistoryQuery query, CancellationToken cancellationToken)
        {
            var history = await _familyRepository.GetPersonRepresentativeHistoryByFamilyAsync(query.FamilyId, cancellationToken);

            var response = history.Select(PersonRepresentativeHistoryResponse.MapToResponse).ToList();

            return ApiResponse<List<PersonRepresentativeHistoryResponse>>.SuccessResult(response);
        }
    }
}
