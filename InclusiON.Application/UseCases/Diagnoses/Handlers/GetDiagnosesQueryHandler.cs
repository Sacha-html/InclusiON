using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, ApiResponse<PagedResponse<DiagnosisListItemResponse>>>
    {
        private readonly IDiagnosesRepository _repository;
        private readonly IEncryptionService   _encryption;

        public GetDiagnosesQueryHandler(IDiagnosesRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<PagedResponse<DiagnosisListItemResponse>>> HandleAsync(
            GetDiagnosesQuery query, CancellationToken cancellationToken)
        {
            var paged = await _repository.GetPagedByPersonIdAsync(query.PersonId, query.Page, query.PageSize, cancellationToken);

            var response = new PagedResponse<DiagnosisListItemResponse>
            {
                Data = paged.Data.Select(d =>
                {
                    var item = DiagnosisListItemResponse.MapToResponse(d);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(d.Id.ToString()));
                    return item;
                }).ToList(),
                TotalRecords    = paged.TotalRecords,
                TotalPages      = paged.TotalPages,
                CurrentPage     = paged.CurrentPage,
                PageSize        = paged.PageSize,
                HasNextPage     = paged.HasNextPage,
                HasPreviousPage = paged.HasPreviousPage
            };

            return ApiResponse<PagedResponse<DiagnosisListItemResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
