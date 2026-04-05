using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetDisabilityTypesQueryHandler
        : IQueryHandler<GetDisabilityTypesQuery, ApiResponse<List<CatalogItemResponse>>>
    {
        private readonly IReadOnlyRepository<DisabilityType> _repository;

        public GetDisabilityTypesQueryHandler(IReadOnlyRepository<DisabilityType> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(
            GetDisabilityTypesQuery query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetAllActiveAsync(cancellationToken);

            var response = items.Select(CatalogItemResponse.MapToResponse).ToList();

            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(response);
        }
    }
}
