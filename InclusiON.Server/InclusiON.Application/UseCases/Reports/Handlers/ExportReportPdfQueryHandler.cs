using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class ExportReportPdfQueryHandler : IQueryHandler<ExportReportPdfQuery, ApiResponse<byte[]>>
    {
        private readonly IReportsRepository _repository;
        private readonly IReportPdfService  _pdfService;

        public ExportReportPdfQueryHandler(IReportsRepository repository, IReportPdfService pdfService)
        {
            _repository = repository;
            _pdfService = pdfService;
        }

        public async Task<ApiResponse<byte[]>> HandleAsync(ExportReportPdfQuery query, CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(query.ReportId, cancellationToken);

            if (report is null)
                return ApiResponse<byte[]>.ErrorResult(ErrorCode.ReportNotFound, ErrorMessages.ReportNotFound);

            var bytes = _pdfService.Generate(report);
            return ApiResponse<byte[]>.SuccessResult(bytes);
        }
    }
}
