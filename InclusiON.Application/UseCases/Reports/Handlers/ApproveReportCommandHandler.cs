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
    public class ApproveReportCommandHandler : ICommandHandler<ApproveReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApproveReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public ApproveReportCommandHandler(
            IReportsRepository repository,
            IFamilyRepository familyRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<ApproveReportCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _familyRepository = familyRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            ApproveReportCommand command,
            CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            if (report.Status != ReportStatus.Submitted)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.InvalidOperation, "Solo se pueden aprobar reportes en estado Enviado.");

            report.Status = ReportStatus.Approved;
            report.ApprovedAt = _dateTime.UtcNow;
            report.ApprovedBy = command.AdminUserId;
            report.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} approved by admin {AdminUserId}", report.Id, command.AdminUserId);

            // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
            // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
            // Notificar a todos los familiares activos vinculados a la persona — fire and forget
            var personId = report.PersonId;
            var reportTitle = report.Title;
            var reportType = report.ReportType?.Name ?? string.Empty;
            var reportDate = report.ReportDate.ToString("dd/MM/yyyy");
            var professionalName = report.Professional != null
                ? $"{report.Professional.FirstName} {report.Professional.LastName}"
                : string.Empty;
            var personName = report.Person != null
                ? $"{report.Person.FirstName} {report.Person.LastName}"
                : string.Empty;
            var year = _dateTime.UtcNow.Year.ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(personId);
                    var activeReps = representatives.Where(r => r.IsActive).ToList();

                    foreach (var rep in activeReps)
                    {
                        // La navegación a User debe estar cargada o se busca por otro medio
                        var familyEmail = rep.Representative?.User?.Email;
                        var familyFirstName = rep.Representative?.FirstName ?? "Familiar";

                        if (string.IsNullOrWhiteSpace(familyEmail)) continue;

                        await _emailService.SendTemplatedEmailAsync(
                            familyEmail,
                            $"Nuevo reporte disponible sobre {personName}",
                            "ReportApproved",
                            new Dictionary<string, string?>
                            {
                                { "FamilyName", familyFirstName },
                                { "PersonName", personName },
                                { "ReportTitle", reportTitle },
                                { "ReportType", reportType },
                                { "ReportDate", reportDate },
                                { "ProfessionalName", professionalName },
                                { "Year", year }
                            });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando emails de aprobación para reporte {ReportId}", command.ReportId);
                }
            });

            return ApiResponse<ReportResponse>.SuccessResult(ReportResponse.MapToResponse(report));
        }
    }
}
