using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Handlers
{
    public class GetInstitutionsQueryHandler
        : IQueryHandler<GetInstitutionsQuery, ApiResponse<PagedResponse<InstitutionResponse>>>
    {
        private readonly IInstitutionsRepository _repository;
        private readonly IEncryptionService      _encryption;

        public GetInstitutionsQueryHandler(IInstitutionsRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<PagedResponse<InstitutionResponse>>> HandleAsync(
            GetInstitutionsQuery query, CancellationToken cancellationToken)
        {
            var paged = await _repository.GetPagedAsync(query.Page, query.PageSize, query.Search, query.IsActive, cancellationToken);

            var response = new PagedResponse<InstitutionResponse>
            {
                Data = paged.Data.Select(i =>
                {
                    var item = InstitutionResponse.MapToResponse(i);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(i.Id.ToString()));
                    return item;
                }).ToList(),
                TotalRecords = paged.TotalRecords,
                TotalPages = paged.TotalPages,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                HasNextPage = paged.HasNextPage,
                HasPreviousPage = paged.HasPreviousPage
            };

            return ApiResponse<PagedResponse<InstitutionResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
