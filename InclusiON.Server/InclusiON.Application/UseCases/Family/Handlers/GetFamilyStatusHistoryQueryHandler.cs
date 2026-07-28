using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyStatusHistoryQueryHandler : IQueryHandler<GetFamilyStatusHistoryQuery, ApiResponse<List<FamilyStatusHistoryResponse>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetFamilyStatusHistoryQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<List<FamilyStatusHistoryResponse>>> HandleAsync(GetFamilyStatusHistoryQuery query, CancellationToken cancellationToken)
        {
            var history = await _familyRepository.GetFamilyStatusHistoryAsync(query.FamilyId, cancellationToken);

            var response = history.Select(FamilyStatusHistoryResponse.MapToResponse).ToList();

            return ApiResponse<List<FamilyStatusHistoryResponse>>.SuccessResult(response);
        }
    }
}
