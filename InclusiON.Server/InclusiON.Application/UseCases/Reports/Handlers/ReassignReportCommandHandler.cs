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
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly INotificationService? _notificationService;

        public ReassignReportCommandHandler(
            IReportsRepository reportsRepository,
            IUnitOfWork unitOfWork,
            ILogger<ReassignReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IEncryptionService encryption,
            IProfessionalsRepository? professionalsRepository = null,
            INotificationService? notificationService = null)
        {
            _reportsRepository       = reportsRepository;
            _unitOfWork              = unitOfWork;
            _logger                  = logger;
            _dateTime                = dateTime;
            _encryption              = encryption;
            _professionalsRepository = professionalsRepository;
            _notificationService     = notificationService;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            ReassignReportCommand command, CancellationToken cancellationToken)
        {
            var report = await _reportsRepository.GetReportWithDetailsAsync(command.ReportId, cancellationToken);

            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            var previousProfessionalId = report.ProfessionalId;

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

            var encryptedReportId = ToUrlSafeBase64(_encryption.Encrypt(report.Id.ToString()));

            if (_notificationService is not null && _professionalsRepository is not null)
            {
                var notifications = new List<NotificationMessage>();
                var newProf = await _professionalsRepository.GetByIdAsync(command.NewProfessionalId, cancellationToken);
                if (newProf is not null && newProf.UserId != Guid.Empty)
                {
                    notifications.Add(new NotificationMessage(
                        UserId: newProf.UserId,
                        Type: NotificationTypes.Report,
                        EventCode: NotificationEventCodes.ReportReassigned,
                        Title: "Reporte reasignado",
                        Message: $"Se te asignó el reporte \"{report.Title}\" para su seguimiento.",
                        ActionUrl: $"/pro/reports/{encryptedReportId}"));
                }

                if (previousProfessionalId != command.NewProfessionalId)
                {
                    var oldProf = await _professionalsRepository.GetByIdAsync(previousProfessionalId, cancellationToken);
                    if (oldProf is not null && oldProf.UserId != Guid.Empty)
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: oldProf.UserId,
                            Type: NotificationTypes.Report,
                            EventCode: NotificationEventCodes.ReportReassigned,
                            Title: "Reporte reasignado",
                            Message: $"El reporte \"{report.Title}\" fue reasignado a otro profesional.",
                            ActionUrl: "/pro/reports"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }

            var response = ReportResponse.MapToResponse(report);
            response.EncryptedId = encryptedReportId;
            return ApiResponse<ReportResponse>.SuccessResult(response, "Reporte reasignado exitosamente.");
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
