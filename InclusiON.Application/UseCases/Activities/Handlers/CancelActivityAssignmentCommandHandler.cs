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
    public class CancelActivityAssignmentCommandHandler
        : ICommandHandler<CancelActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public CancelActivityAssignmentCommandHandler(
            IActivityAssignmentRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CancelActivityAssignmentCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            if (assignment.AssignedByProfessionalId != command.RequestedByProfessionalId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            if (assignment.StatusId != AssignmentStatuses.Pendiente)
                return ApiResponse<ActivityAssignmentResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    $"Solo se puede cancelar una asignación en estado Pendiente. Estado actual: {assignment.Status?.Name ?? assignment.StatusId.ToString()}.");

            assignment.StatusId  = AssignmentStatuses.Cancelada;
            assignment.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(
                ActivityAssignmentResponse.From(updated!),
                "Asignación cancelada.");
        }
    }
}
