using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class MarkReportReadCommandHandler
        : ICommandHandler<MarkReportReadCommand, ApiResponse<object>>
    {
        private readonly IReportsRepository _repository;
        private readonly IUnitOfWork        _unitOfWork;
        private readonly ILogger<MarkReportReadCommandHandler> _logger;

        public MarkReportReadCommandHandler(
            IReportsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<MarkReportReadCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger     = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            MarkReportReadCommand command,
            CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);

            if (report is null)
                return ApiResponse<object>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            if (report.Status != ReportStatus.Approved)
                return ApiResponse<object>.ErrorResult(ErrorCode.InvalidOperation, "Solo se pueden marcar como leídos los reportes aprobados.");

            // Idempotente: si ya está leído, no hace nada
            if (report.IsReadByFamily)
                return ApiResponse<object>.SuccessResult(new { alreadyRead = true });

            report.IsReadByFamily = true;
            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} marked as read by family", command.ReportId);
            return ApiResponse<object>.SuccessResult(new { alreadyRead = false });
        }
    }
}
