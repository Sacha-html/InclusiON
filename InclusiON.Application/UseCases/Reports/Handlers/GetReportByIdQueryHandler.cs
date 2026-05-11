using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using InclusiON.Shared.Resources;
using System.Threading.Tasks;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class GetReportByIdQueryHandler : IQueryHandler<GetReportByIdQuery, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetReportByIdQueryHandler(IReportsRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(GetReportByIdQuery query, CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(query.ReportId, cancellationToken);

            if(report == null)
            {
                return ApiResponse<ReportResponse>.ErrorResult(
                    ErrorCode.ReportNotFound,
                    ErrorMessages.ReportNotFound);
            }

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
