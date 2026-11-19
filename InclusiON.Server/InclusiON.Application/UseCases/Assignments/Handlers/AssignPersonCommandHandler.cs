using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class AssignPersonCommandHandler
        : ICommandHandler<AssignPersonCommand, ApiResponse<ProfessionalPersonResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IRealTimeNotifier _notifier;
        private readonly IRoadmapInitializer _roadmapInitializer;
        private readonly ILogger<AssignPersonCommandHandler> _logger;
        private readonly IFamilyRepository? _familyRepository;
        private readonly INotificationService? _notificationService;

        public AssignPersonCommandHandler(
            IAssignmentsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IRealTimeNotifier notifier,
            IRoadmapInitializer roadmapInitializer,
            ILogger<AssignPersonCommandHandler> logger,
            IFamilyRepository? familyRepository = null,
            INotificationService? notificationService = null)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _notifier = notifier;
            _roadmapInitializer = roadmapInitializer;
            _logger = logger;
            _familyRepository = familyRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<ProfessionalPersonResponse>> HandleAsync(
            AssignPersonCommand command, CancellationToken cancellationToken)
        {
            // Validar que el profesional existe
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            // Validar que el profesional está aprobado
            if (professional.Status != ProfessionalStatusEnum.Approved)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotApproved,
                    ErrorMessages.ProfessionalNotApprovedForPersonAssignment);
            }

            // Validar que la persona existe
            var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (person == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Validar que no exista una asignacion activa
            var existing = await _repository.GetAssignmentAsync(command.ProfessionalId, command.PersonId, cancellationToken);
            if (existing != null && existing.IsActive)
            {
                return ApiResponse<ProfessionalPersonResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    "La asignacion profesional-persona ya existe y esta activa.");
            }

            // Si existe pero esta inactiva, reactivar
            if (existing != null && !existing.IsActive)
            {
                existing.IsActive = true;
                existing.IsPrimaryProfessional = command.IsPrimaryProfessional;
                existing.CanSuperviseLogin = command.CanSuperviseLogin;
                if (command.ClassroomId.HasValue) existing.ClassroomId = command.ClassroomId.Value;
                existing.AssignedAt = _dateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                existing.Person = person;
                if (existing.ClassroomId.HasValue)
                {
                    existing.Classroom = await _repository.GetClassroomByIdAsync(existing.ClassroomId.Value, cancellationToken);
                }
                await NotifyNewStudentAsync(professional, person, cancellationToken);
                await InitializeRoadmapPostCommitAsync(person.Id, professional.UserId, cancellationToken);

                var reactivatedResponse = ProfessionalPersonResponse.MapToResponse(existing);
                return ApiResponse<ProfessionalPersonResponse>.SuccessResult(reactivatedResponse, "Asignacion reactivada exitosamente.");
            }

            var assignment = new ProfessionalPerson
            {
                ProfessionalId = command.ProfessionalId,
                PersonId = command.PersonId,
                ClassroomId = command.ClassroomId,
                IsPrimaryProfessional = command.IsPrimaryProfessional,
                CanSuperviseLogin = command.CanSuperviseLogin,
                AssignedAt = _dateTime.UtcNow,
                IsActive = true
            };

            await _repository.CreateAssignmentAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            assignment.Person = person;
            if (assignment.ClassroomId.HasValue)
            {
                assignment.Classroom = await _repository.GetClassroomByIdAsync(assignment.ClassroomId.Value, cancellationToken);
            }
            await NotifyNewStudentAsync(professional, person, cancellationToken);
            await InitializeRoadmapPostCommitAsync(person.Id, professional.UserId, cancellationToken);

            var response = ProfessionalPersonResponse.MapToResponse(assignment);
            return ApiResponse<ProfessionalPersonResponse>.SuccessResult(response, "Persona asignada al profesional exitosamente.");
        }

        private async Task InitializeRoadmapPostCommitAsync(Guid studentId, Guid professionalUserId, CancellationToken cancellationToken)
        {
            try
            {
                await _roadmapInitializer.InitializeStudentRoadmapAsync(studentId, professionalUserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Alumno {StudentId} asignado, pero falló la inicialización post-commit del roadmap.",
                    studentId);
            }
        }

        private async Task NotifyNewStudentAsync(Professional professional, PersonWithDisability person, CancellationToken cancellationToken)
        {
            var personName = $"{person.FirstName} {person.LastName}".Trim();
            var profName = $"{professional.FirstName} {professional.LastName}".Trim();

            if (_notificationService is null)
            {
                await _notifier.NotifyUserAsync(
                    professional.UserId.ToString(),
                    "🎓 Nuevo alumno asignado",
                    $"Tienes un nuevo alumno, {personName}. Llená el perfil funcional.",
                    actionUrl: $"/pro/persons/{person.Id}",
                    cancellationToken: cancellationToken);
                return;
            }

            var notifications = new List<NotificationMessage>();
            if (professional.UserId != Guid.Empty)
            {
                var encryptedPersonId = _notificationService.EncryptId(person.Id);
                notifications.Add(new NotificationMessage(
                    UserId: professional.UserId,
                    Type: NotificationTypes.Assignment,
                    EventCode: NotificationEventCodes.StudentAssigned,
                    Title: "Nuevo alumno asignado",
                    Message: $"Tenés un nuevo alumno asignado: {personName}. Completá su perfil funcional.",
                    ActionUrl: $"/pro/persons/{encryptedPersonId}"));
            }

            if (_familyRepository is not null)
            {
                var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(person.Id, cancellationToken);
                foreach (var rep in reps.Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty))
                {
                    notifications.Add(new NotificationMessage(
                        UserId: rep.Representative.UserId,
                        Type: NotificationTypes.Assignment,
                        EventCode: NotificationEventCodes.StudentAssigned,
                        Title: "Profesional asignado",
                        Message: $"{profName} fue asignado como profesional de {personName}.",
                        ActionUrl: "/family"));
                }
            }

            await _notificationService.NotifyManyAsync(notifications, cancellationToken);
        }
    }
}
