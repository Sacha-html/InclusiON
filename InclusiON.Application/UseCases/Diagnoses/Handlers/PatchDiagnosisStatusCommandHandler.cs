using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class PatchDiagnosisStatusCommandHandler
        : ICommandHandler<PatchDiagnosisStatusCommand, ApiResponse<object>>
    {
        private readonly IDiagnosesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PatchDiagnosisStatusCommandHandler> _logger;

        public PatchDiagnosisStatusCommandHandler(
            IDiagnosesRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<PatchDiagnosisStatusCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            PatchDiagnosisStatusCommand command, CancellationToken cancellationToken)
        {
            var diagnosis = await _repository.GetByIdIgnoreActiveAsync(command.DiagnosisId, cancellationToken);
            if (diagnosis is null)
                return ApiResponse<object>.ErrorResult(ErrorCode.NotFound, "Diagnóstico no encontrado.");

            if (diagnosis.IsActive == command.IsActive)
            {
                var state = command.IsActive ? "activo" : "inactivo";
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    $"El diagnóstico ya se encuentra {state}.");
            }

            // Si el solicitante es un profesional, solo el creador puede cambiar el estado
            if (command.RequestedByProfessionalId.HasValue &&
                diagnosis.ProfessionalId != command.RequestedByProfessionalId.Value)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.NotAuthorizedForResource,
                    "Solo el profesional que creó el diagnóstico puede modificar su estado.");
            }

            diagnosis.IsActive = command.IsActive;
            await _repository.UpdateAsync(diagnosis, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var action = command.IsActive ? "reactivado" : "dado de baja";
            _logger.LogInformation(
                "Diagnosis {DiagnosisId} {Action} by {RequesterId}",
                diagnosis.Id, action, command.RequestedByProfessionalId?.ToString() ?? "admin");

            return ApiResponse<object>.SuccessResult($"Diagnóstico {action} exitosamente.");
        }
    }
}
