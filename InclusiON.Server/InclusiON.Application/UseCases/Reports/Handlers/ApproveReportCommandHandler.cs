using System.Text.Json;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class ApproveReportCommandHandler : ICommandHandler<ApproveReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApproveReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEncryptionService _encryption;
        private readonly IFamilyRepository? _familyRepository;
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly IBackgroundJobRepository? _backgroundJobs;
        private readonly INotificationService? _notificationService;

        public ApproveReportCommandHandler(
            IReportsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ApproveReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IServiceScopeFactory scopeFactory,
            IEncryptionService encryption,
            IFamilyRepository? familyRepository = null,
            IProfessionalsRepository? professionalsRepository = null,
            IBackgroundJobRepository? backgroundJobs = null,
            INotificationService? notificationService = null)
        {
            _repository              = repository;
            _unitOfWork              = unitOfWork;
            _logger                  = logger;
            _dateTime                = dateTime;
            _scopeFactory            = scopeFactory;
            _encryption              = encryption;
            _familyRepository        = familyRepository;
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _notificationService     = notificationService;
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

            var encryptedReportId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));
            var reportTitle = report.Title;
            var reportType = report.ReportType?.Name ?? string.Empty;
            var reportDate = report.ReportDate.ToString("dd/MM/yyyy");
            var professionalName = report.Professional != null
                ? $"{report.Professional.FirstName} {report.Professional.LastName}".Trim()
                : string.Empty;
            var personName = report.Person != null
                ? $"{report.Person.FirstName} {report.Person.LastName}".Trim()
                : string.Empty;
            var year = _dateTime.UtcNow.Year.ToString();

            // Notificar a todos los tutores activos (in-app persistente + email) y al profesional autor (in-app)
            try
            {
                var notifications = new List<NotificationMessage>();

                if (_familyRepository is not null)
                {
                    var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(report.PersonId, cancellationToken);
                    var activeReps = representatives
                        .Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive)
                        .ToList();

                    foreach (var rep in activeReps)
                    {
                        var familyEmail     = rep.Representative?.User?.Email;
                        var familyFirstName = rep.Representative?.FirstName ?? "Familiar";
                        var familyUserId    = rep.Representative?.UserId;

                        // Notificación in-app persistente SIEMPRE, independientemente de si el familiar tiene email
                        if (familyUserId.HasValue && familyUserId.Value != Guid.Empty)
                        {
                            notifications.Add(new NotificationMessage(
                                UserId: familyUserId.Value,
                                Type: NotificationTypes.Report,
                                EventCode: NotificationEventCodes.ReportApproved,
                                Title: $"Nuevo reporte de {personName}",
                                Message: $"El reporte \"{reportTitle}\" ya está disponible.",
                                ActionUrl: "/family/reports"));
                        }

                        // Email via job queue (solo si tiene email válido)
                        if (_backgroundJobs is not null && !string.IsNullOrWhiteSpace(familyEmail))
                        {
                            await _backgroundJobs.CreateAsync(
                                JobTypes.Email,
                                JsonSerializer.Serialize(new EmailPayload
                                {
                                    To           = familyEmail,
                                    Subject      = $"Nuevo reporte disponible sobre {personName}",
                                    TemplateName = "ReportApproved",
                                    Replacements = new Dictionary<string, string?>
                                    {
                                        { "FamilyName", familyFirstName },
                                        { "PersonName", personName },
                                        { "ReportTitle", reportTitle },
                                        { "ReportType", reportType },
                                        { "ReportDate", reportDate },
                                        { "ProfessionalName", professionalName },
                                        { "Year", year }
                                    }
                                }),
                                maxRetries: 2,
                                cancellationToken: cancellationToken);
                        }
                    }
                }

                // Notificar también al profesional autor que su reporte fue aprobado
                var profUserId = report.Professional?.UserId ?? Guid.Empty;
                if (profUserId == Guid.Empty && _professionalsRepository is not null)
                {
                    var prof = await _professionalsRepository.GetByIdAsync(report.ProfessionalId, cancellationToken);
                    profUserId = prof?.UserId ?? Guid.Empty;
                }

                if (profUserId != Guid.Empty)
                {
                    notifications.Add(new NotificationMessage(
                        UserId: profUserId,
                        Type: NotificationTypes.Report,
                        EventCode: NotificationEventCodes.ReportApproved,
                        Title: "Reporte aprobado",
                        Message: $"Tu reporte \"{reportTitle}\" sobre {personName} fue aprobado y ya está disponible para la familia.",
                        ActionUrl: $"/pro/reports/{encryptedReportId}"));
                }

                if (_notificationService is not null)
                {
                    await _notificationService.NotifyManyAsync(notifications, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encolando notificaciones de aprobación para reporte {ReportId}", command.ReportId);
            }

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = encryptedReportId;
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
