using System.Text.Json;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using ActivityAssignment = InclusiON.Domain.Models.ActivityAssignment;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class AssignFromRoadmapCommandHandler
        : ICommandHandler<AssignFromRoadmapCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IRoadmapRepository             _roadmapRepo;
        private readonly IActivitiesRepository          _activitiesRepo;
        private readonly IActivityAssignmentRepository  _assignmentRepo;
        private readonly IPersonsRepository             _personsRepo;
        private readonly IBackgroundJobRepository       _backgroundJobs;
        private readonly IUnitOfWork                    _unitOfWork;
        private readonly IDateTimeProvider              _dateTime;
        private readonly IEncryptionService             _encryption;
        private readonly IFamilyRepository?             _familyRepository;
        private readonly INotificationService?          _notificationService;

        public AssignFromRoadmapCommandHandler(
            IRoadmapRepository            roadmapRepo,
            IActivitiesRepository         activitiesRepo,
            IActivityAssignmentRepository assignmentRepo,
            IPersonsRepository            personsRepo,
            IBackgroundJobRepository      backgroundJobs,
            IUnitOfWork                   unitOfWork,
            IDateTimeProvider             dateTime,
            IEncryptionService            encryption,
            IFamilyRepository?            familyRepository = null,
            INotificationService?         notificationService = null)
        {
            _roadmapRepo         = roadmapRepo;
            _activitiesRepo      = activitiesRepo;
            _assignmentRepo      = assignmentRepo;
            _personsRepo         = personsRepo;
            _backgroundJobs      = backgroundJobs;
            _unitOfWork          = unitOfWork;
            _dateTime            = dateTime;
            _encryption          = encryption;
            _familyRepository    = familyRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            AssignFromRoadmapCommand command, CancellationToken cancellationToken)
        {
            var entry = await _roadmapRepo.GetActivityByIdAsync(
                command.PersonRoadmapActivityId, cancellationToken);

            if (entry is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Actividad del roadmap");

            var activity = await _activitiesRepo.GetByIdAsync(entry.ActivityId, cancellationToken);

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
                var hasActiveAssignment = await _assignmentRepo.HasActiveAssignmentAsync(command.PersonId, entry.ActivityId, cancellationToken);

                if (hasActiveAssignment)
                {
                    return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                        ErrorCode.Conflict,
                        "El alumno ya posee una asignación activa para esta actividad.");
                }
            }

            var assignment = new ActivityAssignment
            {
                ActivityId               = entry.ActivityId,
                PersonId                 = command.PersonId,
                AssignedByProfessionalId = command.AssignedByProfessionalId,
                AssignedAt               = _dateTime.UtcNow,
                DueDate                  = command.DueDate,
                StatusId                 = AssignmentStatuses.Pendiente,
                IsEvaluationActivity     = command.IsEvaluationActivity,
                CreatedAt                = _dateTime.UtcNow,
            };

            await _assignmentRepo.CreateAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notificar a todos los tutores activos del alumno (persistente + tiempo real)
            if (_notificationService is not null && _familyRepository is not null)
            {
                var person = await _personsRepo.GetByIdAsync(command.PersonId, cancellationToken);
                var studentName = person is not null ? $"{person.FirstName} {person.LastName}".Trim() : "tu alumno a cargo";
                var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(command.PersonId, cancellationToken);
                var tutorNotifications = reps
                    .Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty)
                    .Select(r => new NotificationMessage(
                        UserId: r.Representative.UserId,
                        Type: NotificationTypes.Activity,
                        EventCode: NotificationEventCodes.ActivityAssigned,
                        Title: "Nueva actividad asignada",
                        Message: $"Se asignó la actividad \"{activity.Title}\" a {studentName}.",
                        ActionUrl: "/family/activities"));

                await _notificationService.NotifyManyAsync(tutorNotifications, cancellationToken);
            }

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
    }
}
