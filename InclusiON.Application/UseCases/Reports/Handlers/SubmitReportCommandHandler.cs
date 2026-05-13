using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class SubmitReportCommandHandler : ICommandHandler<SubmitReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubmitReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public SubmitReportCommandHandler(
            IReportsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IUnitOfWork unitOfWork,
            ILogger<SubmitReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            SubmitReportCommand command,
            CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            // Solo el profesional autor puede enviar
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional is null || report.ProfessionalId != professional.Id)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.Forbidden, "No tenés permiso para enviar este reporte.");

            if (report.Status != ReportStatus.Draft)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.InvalidOperation, "Solo se pueden enviar reportes en estado Borrador.");

            report.Status = ReportStatus.Submitted;
            report.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} submitted by professional {ProfessionalId}", report.Id, command.ProfessionalId);

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
