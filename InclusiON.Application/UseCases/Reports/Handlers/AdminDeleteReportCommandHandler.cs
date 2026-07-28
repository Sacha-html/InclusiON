using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class AdminDeleteReportCommandHandler : ICommandHandler<AdminDeleteReportCommand, ApiResponse<object>>
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDeleteReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public AdminDeleteReportCommandHandler(
            IReportsRepository reportsRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminDeleteReportCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _reportsRepository = reportsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            AdminDeleteReportCommand command, CancellationToken cancellationToken)
        {
            var report = await _reportsRepository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<object>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            if (!report.IsActive)
                return ApiResponse<object>.ErrorResult(ErrorCode.InvalidOperation, "El reporte ya se encuentra inactivo.");

            await _reportsRepository.SoftDeleteReportAsync(report, _dateTime.UtcNow, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} soft-deleted by admin {AdminUserId}", report.Id, command.AdminUserId);

            return ApiResponse<object>.SuccessResult("Reporte dado de baja exitosamente.");
        }
    }
}
