using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using ActivityResponse = InclusiON.Domain.Models.ActivityResponse;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class StartActivityResponseCommandHandler
        : ICommandHandler<StartActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public StartActivityResponseCommandHandler(
            IActivityAssignmentRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            StartActivityResponseCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            if (assignment.PersonId != command.PersonId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            if (assignment.StatusId == AssignmentStatuses.Completada || assignment.StatusId == AssignmentStatuses.Cancelada)
                return ApiResponse<ActivityAssignmentResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    $"No se puede iniciar una actividad en estado {assignment.Status?.Name ?? assignment.StatusId.ToString()}.");

            var attemptCount = await _repository.CountResponsesAsync(command.AssignmentId, cancellationToken);

            var response = new ActivityResponse
            {
                AssignmentId = command.AssignmentId,
                StartedAt    = _dateTime.UtcNow,
                AttemptCount = attemptCount + 1,
                CreatedAt    = _dateTime.UtcNow,
            };

            await _repository.CreateResponseAsync(response, cancellationToken);

            if (assignment.StatusId == AssignmentStatuses.Pendiente)
            {
                assignment.StatusId  = AssignmentStatuses.EnProgreso;
                assignment.UpdatedAt = _dateTime.UtcNow;
                await _repository.UpdateAsync(assignment, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(
                ActivityAssignmentResponse.From(updated!),
                "Actividad iniciada.");
        }
    }
}
