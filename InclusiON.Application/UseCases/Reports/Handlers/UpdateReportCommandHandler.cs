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
    public class UpdateReportCommandHandler : ICommandHandler<UpdateReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public UpdateReportCommandHandler(
            IReportsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateReportCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            UpdateReportCommand command,
            CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            // Solo el profesional autor puede editar
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional is null || report.ProfessionalId != professional.Id)
                return ApiResponse<ReportResponse>.ErrorResult(ErrorCode.Forbidden, "No tenés permiso para editar este reporte.");

            // Solo se puede editar si está en borrador o rechazado
            if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.Rejected)
                return ApiResponse<ReportResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    $"No se puede editar un reporte en estado '{report.Status}'. Solo se permiten ediciones en Borrador o Rechazado.");

            report.Title = command.Title;
            report.Content = command.Content;
            report.ReportTypeId = command.ReportTypeId;
            report.ReportDate = command.ReportDate;
            report.PeriodStartDate = command.PeriodStartDate;
            report.PeriodEndDate = command.PeriodEndDate;
            report.AchievedGoals = command.AchievedGoals;
            report.AreasToReinforce = command.AreasToReinforce;
            report.FutureRecommendations = command.FutureRecommendations;
            report.NextObjectives = command.NextObjectives;
            report.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} updated by professional {ProfessionalId}", report.Id, command.ProfessionalId);

            return ApiResponse<ReportResponse>.SuccessResult(ReportResponse.MapToResponse(report));
        }
    }
}
