using System.Text.Json;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
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
        private readonly IPersonsRepository _personsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public CreateActivityAssignmentCommandHandler(
            IActivityAssignmentRepository repository,
            IActivitiesRepository activitiesRepository,
            IPersonsRepository personsRepository,
            IProfessionalsRepository professionalsRepository,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository              = repository;
            _activitiesRepository    = activitiesRepository;
            _personsRepository       = personsRepository;
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _unitOfWork              = unitOfWork;
            _dateTime                = dateTime;
            _encryption              = encryption;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CreateActivityAssignmentCommand command, CancellationToken cancellationToken)
        {
            var professional = await _professionalsRepository.GetByIdAsync(command.AssignedByProfessionalId, cancellationToken);
            if (professional is null || !professional.IsActive)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            if (!int.TryParse(_encryption.Decrypt(ToStandardBase64(command.EncryptedActivityId)), out var activityId))
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(ErrorCode.ValidationFailed, "Identificador de actividad inválido.");

            var activity = await _activitiesRepository.GetByIdAsync(activityId, cancellationToken);

            if (activity is null || !activity.IsActive)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Actividad");

            if (!activity.IsStandardActivity && activity.ProfessionalId != command.AssignedByProfessionalId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            if (command.DueDate.HasValue && command.DueDate.Value.Date < _dateTime.UtcNow.Date)
            {
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "La fecha límite no puede ser anterior a la fecha actual.");
            }

            if (!command.BypassDuplicateWarning)
            {
                var hasActiveAssignment = await _repository.HasActiveAssignmentAsync(command.PersonId, activityId, cancellationToken);

                if (hasActiveAssignment)
                {
                    return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                        ErrorCode.Conflict,
                        "El alumno ya posee una asignación activa para esta actividad.");
                }
            }

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

            // Notificar a la persona — fire and forget (job INSERT rápido)
            _ = Task.Run(async () =>
            {
                try
                {
                    var person = await _personsRepository.GetByIdAsync(command.PersonId, CancellationToken.None);
                    if (person is not null)
                    {
                        await _backgroundJobs.CreateAsync(
                            JobTypes.Push,
                            JsonSerializer.Serialize(new NotificationPayload
                            {
                                UserId    = person.UserId.ToString(),
                                Title     = "Nueva actividad asignada",
                                Message   = $"Tenés una nueva actividad: {activity.Title}",
                                ActionUrl = "/#/app/activities"
                            }),
                            maxRetries: 3);
                    }
                }
                catch { /* fire and forget — no bloquea respuesta */ }
            });

            // Build response from in-memory data — avoids re-fetching after save
            var dto = new ActivityAssignmentResponse
            {
                Id                   = assignment.Id,
                ActivityId           = assignment.ActivityId,
                ActivityTitle        = activity.Title,
                TemplateTypeCode     = activity.Content?.TemplateType?.Code ?? string.Empty,
                ContentJson          = activity.Content?.ContentJson ?? string.Empty,
                PersonId             = assignment.PersonId,
                Status               = "Pendiente",
                AssignedAt           = assignment.AssignedAt,
                DueDate              = assignment.DueDate,
                IsEvaluationActivity = assignment.IsEvaluationActivity,
                Responses            = [],
            };
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(assignment.Id.ToString()));
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
