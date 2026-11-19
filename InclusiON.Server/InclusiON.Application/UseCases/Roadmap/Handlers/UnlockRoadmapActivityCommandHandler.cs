using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class UnlockRoadmapActivityCommandHandler
        : ICommandHandler<UnlockRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>>
    {
        private readonly IRoadmapRepository            _roadmaps;
        private readonly IActivityAssignmentRepository _assignments;
        private readonly IUnitOfWork                   _uow;
        private readonly IDateTimeProvider             _dateTime;
        private readonly IEncryptionService            _encryption;
        private readonly IPersonsRepository?           _personsRepository;
        private readonly IFamilyRepository?            _familyRepository;
        private readonly INotificationService?         _notificationService;

        public UnlockRoadmapActivityCommandHandler(
            IRoadmapRepository            roadmaps,
            IActivityAssignmentRepository assignments,
            IUnitOfWork                   uow,
            IDateTimeProvider             dateTime,
            IEncryptionService            encryption,
            IPersonsRepository?           personsRepository = null,
            IFamilyRepository?            familyRepository = null,
            INotificationService?         notificationService = null)
        {
            _roadmaps            = roadmaps;
            _assignments         = assignments;
            _uow                 = uow;
            _dateTime            = dateTime;
            _encryption          = encryption;
            _personsRepository   = personsRepository;
            _familyRepository    = familyRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<RoadmapActivityResponse>> HandleAsync(
            UnlockRoadmapActivityCommand command, CancellationToken cancellationToken)
        {
            var entry = await _roadmaps.GetActivityByIdAsync(command.ActivityEntryId, cancellationToken);

            if (entry is null)
                return ApiResponse<RoadmapActivityResponse>.NotFound("Actividad del roadmap");

            if (entry.IsUnlocked)
                return ApiResponse<RoadmapActivityResponse>.Conflict(
                    ErrorCode.Conflict,
                    "La actividad ya esta desbloqueada.");

            entry.IsUnlocked = true;
            entry.UnlockedAt = _dateTime.UtcNow;

            // Crear la asignación correspondiente para que la persona pueda realizar la actividad
            var assignment = new ActivityAssignment
            {
                ActivityId               = entry.ActivityId,
                PersonId                 = command.PersonId,
                AssignedByProfessionalId = command.ProfessionalId,
                AssignedAt               = _dateTime.UtcNow,
                StatusId                 = AssignmentStatuses.Pendiente,
                CreatedAt                = _dateTime.UtcNow,
            };
            await _assignments.CreateAsync(assignment, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            // Notificar a todos los tutores activos del alumno sobre el nuevo nivel desbloqueado
            if (_notificationService is not null && _familyRepository is not null)
            {
                var person = _personsRepository is not null
                    ? await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken)
                    : null;
                var studentName = person is not null ? $"{person.FirstName} {person.LastName}".Trim() : "tu alumno a cargo";
                var activityTitle = entry.Activity?.Title ?? "una nueva estación";
                var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(command.PersonId, cancellationToken);
                var tutorNotifications = reps
                    .Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty)
                    .Select(r => new NotificationMessage(
                        UserId: r.Representative.UserId,
                        Type: NotificationTypes.Activity,
                        EventCode: NotificationEventCodes.RoadmapUnlocked,
                        Title: "Nuevo nivel desbloqueado",
                        Message: $"Se desbloqueó \"{activityTitle}\" en Mi Camino para {studentName}.",
                        ActionUrl: "/family"));

                await _notificationService.NotifyManyAsync(tutorNotifications, cancellationToken);
            }

            var actDto = RoadmapMapper.ToActivityResponse(entry);
            actDto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(entry.Id.ToString()));
            actDto.EncryptedActivityId = ToUrlSafeBase64(_encryption.Encrypt(entry.ActivityId.ToString()));
            return ApiResponse<RoadmapActivityResponse>.SuccessResult(actDto, "Actividad desbloqueada y asignada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
