using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class CompleteActivityResponseCommandHandler
        : ICommandHandler<CompleteActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public CompleteActivityResponseCommandHandler(
            IActivityAssignmentRepository repository,
            IRoadmapRepository roadmapRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _roadmapRepository = roadmapRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CompleteActivityResponseCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            if (assignment.PersonId != command.PersonId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            var response = await _repository.GetResponseByIdAsync(command.ResponseId, cancellationToken);

            if (response is null || response.AssignmentId != command.AssignmentId)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Respuesta");

            if (response.CompletedAt.HasValue)
                return ApiResponse<ActivityAssignmentResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    "Esta respuesta ya fue completada.");

            var now = _dateTime.UtcNow;

            response.CompletedAt       = now;
            response.TimeSpentSeconds  = command.TimeSpentSeconds;
            response.SuccessPercentage = command.SuccessPercentage;
            response.Result            = ResolveResult(command.SuccessPercentage);
            response.RequiredSupport   = command.RequiredSupport;
            response.FrustrationLevel  = command.FrustrationLevel;
            response.ResponsePattern   = command.ResponsePattern;
            response.Observations      = command.Observations;
            response.UpdatedAt         = now;

            await _repository.UpdateResponseAsync(response, cancellationToken);

            assignment.StatusId  = AssignmentStatuses.Completada;
            assignment.UpdatedAt = now;
            await _repository.UpdateAsync(assignment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Auto-unlock next roadmap activity if threshold met
            var roadmapEntry = await _roadmapRepository.GetByPersonAndActivityAsync(
                assignment.PersonId, assignment.ActivityId, cancellationToken);

            if (roadmapEntry is not null && command.SuccessPercentage >= roadmapEntry.UnlockThresholdPercent)
            {
                var next = await _roadmapRepository.GetNextInAreaAsync(
                    roadmapEntry.PersonRoadmapAreaId, roadmapEntry.SequenceOrder, cancellationToken);

                if (next is not null && !next.IsUnlocked)
                {
                    next.IsUnlocked = true;
                    next.UnlockedAt = now;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            var updated = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(
                ActivityAssignmentResponse.From(updated!),
                "Actividad completada.");
        }

        private static ActivityResponseResult ResolveResult(decimal successPercentage) =>
            successPercentage >= 80 ? ActivityResponseResult.Exito
            : successPercentage >= 50 ? ActivityResponseResult.Parcial
            : ActivityResponseResult.Fallido;
    }
}
