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
    public class RejectReportCommandHandler : ICommandHandler<RejectReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RejectReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public RejectReportCommandHandler(
            IReportsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<RejectReportCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            RejectReportCommand command,
            CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            if (report.Status != ReportStatus.Submitted)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.InvalidOperation, "Solo se pueden rechazar reportes en estado Enviado.");

            if (string.IsNullOrWhiteSpace(command.Comment))
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.InvalidOperation, "El motivo del rechazo es obligatorio.");

            report.Status = ReportStatus.Rejected;
            report.AdminComment = command.Comment.Trim();
            report.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} rejected by admin {AdminUserId}", report.Id, command.AdminUserId);

            // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
            // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
            // Notificar al profesional autor — fire and forget
            var reportId = report.Id;
            var reportTitle = report.Title;
            var reportDate = report.ReportDate.ToString("dd/MM/yyyy");
            var adminComment = report.AdminComment;
            var personName = report.Person != null
                ? $"{report.Person.FirstName} {report.Person.LastName}"
                : string.Empty;
            var professionalId = report.ProfessionalId;
            var year = _dateTime.UtcNow.Year.ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    var professional = await _professionalsRepository.GetByIdAsync(professionalId);
                    var professionalEmail = professional?.User?.Email ?? professional?.Email;
                    var professionalName = professional != null
                        ? $"{professional.FirstName} {professional.LastName}"
                        : string.Empty;

                    if (string.IsNullOrWhiteSpace(professionalEmail)) return;

                    await _emailService.SendTemplatedEmailAsync(
                        professionalEmail,
                        "Tu reporte requiere correcciones",
                        "ReportRejected",
                        new Dictionary<string, string?>
                        {
                            { "ProfessionalName", professionalName },
                            { "ReportTitle", reportTitle },
                            { "PersonName", personName },
                            { "ReportDate", reportDate },
                            { "AdminComment", adminComment },
                            { "Year", year }
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando email de rechazo para reporte {ReportId}", reportId);
                }
            });

            return ApiResponse<ReportResponse>.SuccessResult(ReportResponse.MapToResponse(report));
        }
    }
}
