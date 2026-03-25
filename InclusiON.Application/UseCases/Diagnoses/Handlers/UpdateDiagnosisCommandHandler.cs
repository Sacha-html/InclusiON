using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class UpdateDiagnosisCommandHandler : ICommandHandler<UpdateDiagnosisCommand, ApiResponse<DiagnosisResponse>>
    {
        private readonly IDiagnosesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateDiagnosisCommandHandler> _logger;

        public UpdateDiagnosisCommandHandler(
            IDiagnosesRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateDiagnosisCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<DiagnosisResponse>> HandleAsync(
            UpdateDiagnosisCommand command, CancellationToken cancellationToken)
        {
            var diagnosis = await _repository.GetByIdAsync(command.DiagnosisId, cancellationToken);
            if (diagnosis is null)
                return ApiResponse<DiagnosisResponse>.NotFound("Diagnóstico");

            // Solo el creador puede editar
            if (diagnosis.ProfessionalId != command.RequestedByProfessionalId)
            {
                return ApiResponse<DiagnosisResponse>.ErrorResult(
                    ErrorCode.NotAuthorizedForResource,
                    "Solo el profesional que creó el diagnóstico puede editarlo.");
            }

            diagnosis.DiagnosisDate = command.DiagnosisDate;
            diagnosis.PrimaryDiagnosis = command.PrimaryDiagnosis;
            diagnosis.InitialObservations = command.InitialObservations;
            diagnosis.IdentifiedCapabilities = command.IdentifiedCapabilities;
            diagnosis.IdentifiedChallenges = command.IdentifiedChallenges;
            diagnosis.RequiredSupports = command.RequiredSupports;
            diagnosis.PedagogicalObjectives = command.PedagogicalObjectives;
            diagnosis.RecommendedStrategies = command.RecommendedStrategies;
            diagnosis.UpdatedAt = DateTime.UtcNow;
            diagnosis.UpdatedBy = command.RequestedByProfessionalId;

            await _repository.UpdateAsync(diagnosis, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Diagnosis updated: {DiagnosisId} by professional {ProfessionalId}",
                command.DiagnosisId, command.RequestedByProfessionalId);

            return ApiResponse<DiagnosisResponse>.SuccessResult(
                GetDiagnosisByIdQueryHandler.MapToResponse(diagnosis),
                "Diagnóstico actualizado exitosamente.");
        }
    }
}
