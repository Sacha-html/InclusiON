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
        private readonly IEncryptionService _encryption;

        public CreateActivityAssignmentCommandHandler(
            IActivityAssignmentRepository repository,
            IActivitiesRepository activitiesRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _activitiesRepository = activitiesRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CreateActivityAssignmentCommand command, CancellationToken cancellationToken)
        {
            if (!int.TryParse(_encryption.Decrypt(ToStandardBase64(command.EncryptedActivityId)), out var activityId))
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(ErrorCode.ValidationFailed, "Identificador de actividad inválido.");

            var activity = await _activitiesRepository.GetByIdAsync(activityId, cancellationToken);

            if (activity is null || !activity.IsActive)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Actividad");

            if (!activity.IsStandardActivity && activity.ProfessionalId != command.AssignedByProfessionalId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            var assignment = new ActivityAssignment
            {
                ActivityId               = activityId,
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

            var dto = ActivityAssignmentResponse.From(created!);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(created!.Id.ToString()));
            foreach (var attempt in dto.Responses)
                attempt.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(attempt.Id.ToString()));
            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(dto, "Actividad asignada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');

        private static string ToStandardBase64(string urlSafe)
        {
            var s = urlSafe.Replace('-', '+').Replace('_', '/');
            return (s.Length % 4) switch { 2 => s + "==", 3 => s + "=", _ => s };
        }
    }
}
