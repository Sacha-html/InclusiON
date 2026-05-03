using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using ActivityAssignment = InclusiON.Domain.Models.ActivityAssignment;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class CreateActivityAssignmentCommandHandler
        : ICommandHandler<CreateActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public CreateActivityAssignmentCommandHandler(
            IActivityAssignmentRepository repository,
            IActivitiesRepository activitiesRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _activitiesRepository = activitiesRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CreateActivityAssignmentCommand command, CancellationToken cancellationToken)
        {
            var activity = await _activitiesRepository.GetByIdAsync(command.ActivityId, cancellationToken);

            if (activity is null || !activity.IsActive)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Actividad");

            if (!activity.IsStandardActivity && activity.ProfessionalId != command.AssignedByProfessionalId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            var assignment = new ActivityAssignment
            {
                ActivityId               = command.ActivityId,
                PersonId                 = command.PersonId,
                AssignedByProfessionalId = command.AssignedByProfessionalId,
                AssignedAt               = _dateTime.UtcNow,
                DueDate                  = command.DueDate,
                StatusId                 = AssignmentStatuses.Pendiente,
                IsEvaluationActivity     = command.IsEvaluationActivity,
                SequenceOrder            = command.SequenceOrder,
                CreatedAt                = _dateTime.UtcNow,
            };

            await _repository.CreateAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await _repository.GetByIdAsync(assignment.Id, cancellationToken);

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(
                ActivityAssignmentResponse.From(created!),
                "Actividad asignada exitosamente.");
        }
    }
}
