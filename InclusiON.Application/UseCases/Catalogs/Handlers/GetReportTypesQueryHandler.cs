using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetReportTypesQueryHandler
        : IQueryHandler<GetReportTypesQuery, ApiResponse<List<CatalogItemResponse>>>
    {
        private readonly IReadOnlyRepository<ReportType> _repository;

        public GetReportTypesQueryHandler(IReadOnlyRepository<ReportType> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(
            GetReportTypesQuery query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x => CatalogItemResponse.MapToResponse(x)).ToList();
            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(response);
        }
    }
}
