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
    public class RejectReportCommandHandler : ICommandHandler<RejectReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RejectReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEncryptionService _encryption;
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly IBackgroundJobRepository? _backgroundJobs;
        private readonly INotificationService? _notificationService;

        public RejectReportCommandHandler(
            IReportsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<RejectReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IServiceScopeFactory scopeFactory,
            IEncryptionService encryption,
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
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _notificationService     = notificationService;
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

            var encryptedReportId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));
            var reportTitle = report.Title;
            var reportDate = report.ReportDate.ToString("dd/MM/yyyy");
            var adminComment = report.AdminComment;
            var personName = report.Person != null
                ? $"{report.Person.FirstName} {report.Person.LastName}".Trim()
                : string.Empty;
            var year = _dateTime.UtcNow.Year.ToString();

            // Notificar exclusivamente al profesional autor (in-app persistente + email)
            try
            {
                var professional = report.Professional
                    ?? (_professionalsRepository is not null
                        ? await _professionalsRepository.GetByIdAsync(report.ProfessionalId, cancellationToken)
                        : null);

                if (professional is not null)
                {
                    if (_notificationService is not null && professional.UserId != Guid.Empty)
                    {
                        await _notificationService.NotifyAsync(new NotificationMessage(
                            UserId: professional.UserId,
                            Type: NotificationTypes.Report,
                            EventCode: NotificationEventCodes.ReportRejected,
                            Title: "Reporte rechazado",
                            Message: $"Tu reporte \"{reportTitle}\" requiere correcciones. Revisá los comentarios del administrador.",
                            ActionUrl: $"/pro/reports/{encryptedReportId}/edit"),
                            cancellationToken);
                    }

                    var professionalEmail = professional.User?.Email ?? professional.Email;
                    if (_backgroundJobs is not null && !string.IsNullOrWhiteSpace(professionalEmail))
                    {
                        var professionalName = $"{professional.FirstName} {professional.LastName}".Trim();
                        await _backgroundJobs.CreateAsync(
                            JobTypes.Email,
                            JsonSerializer.Serialize(new EmailPayload
                            {
                                To           = professionalEmail,
                                Subject      = "Tu reporte requiere correcciones",
                                TemplateName = "ReportRejected",
                                Replacements = new Dictionary<string, string?>
                                {
                                    { "ProfessionalName", professionalName },
                                    { "ReportTitle", reportTitle },
                                    { "PersonName", personName },
                                    { "ReportDate", reportDate },
                                    { "AdminComment", adminComment },
                                    { "Year", year }
                                }
                            }),
                            maxRetries: 2,
                            cancellationToken: cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encolando notificaciones de rechazo para reporte {ReportId}", report.Id);
            }

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = encryptedReportId;
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
