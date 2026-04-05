using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetAvailableFamiliesQueryHandler : IQueryHandler<GetAvailableFamiliesQuery, ApiResponse<List<FamilyResponse>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetAvailableFamiliesQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<List<FamilyResponse>>> HandleAsync(GetAvailableFamiliesQuery query, CancellationToken cancellationToken)
        {
            var families = await _familyRepository.GetAvailableFamiliesAsync(query.Search, query.PersonId, cancellationToken);

            var response = families.Select(f => FamilyResponse.MapToResponse(f.Family, f.WasPreviouslyLinked)).ToList();

            return ApiResponse<List<FamilyResponse>>.SuccessResult(response);
        }
    }
}
