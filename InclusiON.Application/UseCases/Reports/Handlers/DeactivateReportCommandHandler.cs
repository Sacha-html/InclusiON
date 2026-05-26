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
    public class DeactivateReportCommandHandler : ICommandHandler<DeactivateReportCommand, ApiResponse<object>>
    {
        private readonly IReportsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateReportCommandHandler> _logger;

        public DeactivateReportCommandHandler(
            IReportsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateReportCommandHandler> logger)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            DeactivateReportCommand command, CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(command.ReportId, cancellationToken);
            if (report is null)
                return ApiResponse<object>.ErrorResult(ErrorCode.ReportNotFound, "Reporte no encontrado.");

            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional is null || report.ProfessionalId != professional.Id)
                return ApiResponse<object>.ErrorResult(ErrorCode.Forbidden, "No tenés permiso para dar de baja este reporte.");

            if (!report.IsActive)
                return ApiResponse<object>.ErrorResult(ErrorCode.InvalidOperation, "El reporte ya se encuentra inactivo.");

            if (report.Status == ReportStatus.Submitted)
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "No se puede dar de baja un reporte en estado 'Enviado'. Esperá la revisión del administrador.");

            report.IsActive = false;
            await _repository.UpdateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportId} deactivated by professional {ProfessionalId}", report.Id, command.ProfessionalId);

            return ApiResponse<object>.SuccessResult("Reporte dado de baja exitosamente.");
        }
    }
}
