using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyByIdQueryHandler : IQueryHandler<GetFamilyByIdQuery, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;

        public GetFamilyByIdQueryHandler(IFamilyRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(GetFamilyByIdQuery query, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(query.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            var response = GetFamilyByIdQuery.MapToResponse(family);
            return ApiResponse<FamilyResponse>.SuccessResult(response);
        }
    }
}
