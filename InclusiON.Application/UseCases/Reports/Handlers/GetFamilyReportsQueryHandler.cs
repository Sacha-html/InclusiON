using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
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
        private readonly IEncryptionService _encryption;

        public GetFamilyReportsQueryHandler(IReportsRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
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
                Data = pagedResult.Data.Select(r =>
                {
                    var item = ReportsListItemReponse.MapToResponse(r);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(r.Id.ToString()));
                    return item;
                }).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<ReportsListItemReponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
