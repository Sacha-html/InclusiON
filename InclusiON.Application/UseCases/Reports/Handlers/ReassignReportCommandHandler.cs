using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class ReassignReportCommandHandler : ICommandHandler<ReassignReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReassignReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public ReassignReportCommandHandler(
            IReportsRepository reportsRepository,
            IUnitOfWork unitOfWork,
            ILogger<ReassignReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _reportsRepository = reportsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            ReassignReportCommand command, CancellationToken cancellationToken)
        {
            var report = await _reportsRepository.GetReportWithDetailsAsync(command.ReportId, cancellationToken);

            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            try
            {
                await _reportsRepository.ReassignReportAsync(report, command.NewProfessionalId, _dateTime.UtcNow, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ProfessionalNotFound, ex.Message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} reassigned to professional {NewProfessionalId} by admin {AdminUserId}", report.Id, command.NewProfessionalId, command.AdminUserId);

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));
            return ApiResponse<ReportResponse>.SuccessResult(response, "Reporte reasignado exitosamente.");
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
