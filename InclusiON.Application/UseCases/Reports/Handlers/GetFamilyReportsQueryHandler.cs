using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class GetFamilyReportsQueryHandler
        : IQueryHandler<GetFamilyReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>>
    {
        private readonly IReportsRepository _repository;

        public GetFamilyReportsQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<ReportsListItemReponse>>> HandleAsync(
            GetFamilyReportsQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _repository.GetFamilyPagedAsync(
                query.FamilyRepresentativeId,
                query.Page,
                query.PageSize,
                query.ReportTypeId,
                query.DateFrom,
                query.DateTo,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

            var response = new PagedResponse<ReportsListItemReponse>
            {
                Data = pagedResult.Data.Select(ReportsListItemReponse.MapToResponse).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<ReportsListItemReponse>>.SuccessResult(response);
        }
    }
}
