using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class CreateDiagnosisCommandHandler : ICommandHandler<CreateDiagnosisCommand, ApiResponse<DiagnosisResponse>>
    {
        private readonly IDiagnosesRepository _repository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateDiagnosisCommandHandler> _logger;

        public CreateDiagnosisCommandHandler(
            IDiagnosesRepository repository,
            IPersonsRepository personsRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateDiagnosisCommandHandler> logger)
        {
            _repository = repository;
            _personsRepository = personsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<DiagnosisResponse>> HandleAsync(
            CreateDiagnosisCommand command, CancellationToken cancellationToken)
        {
            var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<DiagnosisResponse>.ErrorResult(ErrorCode.PersonNotFound, "Persona no encontrada.");

            var diagnosis = new Diagnosis
            {
                PersonId = command.PersonId,
                ProfessionalId = command.ProfessionalId,
                DiagnosisDate = command.DiagnosisDate,
                PrimaryDiagnosis = command.PrimaryDiagnosis,
                InitialObservations = command.InitialObservations,
                IdentifiedCapabilities = command.IdentifiedCapabilities,
                IdentifiedChallenges = command.IdentifiedChallenges,
                RequiredSupports = command.RequiredSupports,
                PedagogicalObjectives = command.PedagogicalObjectives,
                RecommendedStrategies = command.RecommendedStrategies,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.ProfessionalId,
                IsActive = true
            };

            await _repository.CreateAsync(diagnosis, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Diagnosis created: {DiagnosisId} for person {PersonId} by professional {ProfessionalId}",
                diagnosis.Id, command.PersonId, command.ProfessionalId);

            // Recargar con includes para el response
            var created = await _repository.GetByIdAsync(diagnosis.Id, cancellationToken);
            return ApiResponse<DiagnosisResponse>.SuccessResult(
                GetDiagnosisByIdQueryHandler.MapToResponse(created!),
                "Diagnóstico creado exitosamente.");
        }
    }
}
