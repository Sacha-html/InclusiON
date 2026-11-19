using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class DeactivatePersonAssignmentCommandHandler
        : ICommandHandler<DeactivatePersonAssignmentCommand, ApiResponse<ProfessionalPersonResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly IFamilyRepository? _familyRepository;
        private readonly INotificationService? _notificationService;

        public DeactivatePersonAssignmentCommandHandler(
            IAssignmentsRepository repository,
            IUnitOfWork unitOfWork,
            IProfessionalsRepository? professionalsRepository = null,
            IFamilyRepository? familyRepository = null,
            INotificationService? notificationService = null)
        {
            _repository              = repository;
            _unitOfWork              = unitOfWork;
            _professionalsRepository = professionalsRepository;
            _familyRepository        = familyRepository;
            _notificationService     = notificationService;
        }

        public async Task<ApiResponse<ProfessionalPersonResponse>> HandleAsync(
            DeactivatePersonAssignmentCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetAssignmentAsync(command.ProfessionalId, command.PersonId, cancellationToken);

            if (assignment == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.NotFound("Asignacion profesional-persona");
            }

            if (!assignment.IsActive)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "La asignacion ya se encuentra inactiva.");
            }

            assignment.IsActive = false;

            await _repository.CancelActiveAssignmentsForProfessionalAndPersonAsync(command.ProfessionalId, command.PersonId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (_notificationService is not null)
            {
                var notifications = new List<NotificationMessage>();
                var studentName = assignment.Person != null
                    ? $"{assignment.Person.FirstName} {assignment.Person.LastName}".Trim()
                    : "un alumno";

                var prof = _professionalsRepository is not null
                    ? await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken)
                    : null;
                var profName = prof != null ? $"{prof.FirstName} {prof.LastName}".Trim() : "Un profesional";

                if (prof is not null && prof.UserId != Guid.Empty)
                {
                    notifications.Add(new NotificationMessage(
                        UserId: prof.UserId,
                        Type: NotificationTypes.Assignment,
                        EventCode: NotificationEventCodes.StudentUnassigned,
                        Title: "Alumno desvinculado",
                        Message: $"El alumno {studentName} fue desvinculado de tu aula.",
                        ActionUrl: "/pro/persons"));
                }

                if (_familyRepository is not null)
                {
                    var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(command.PersonId, cancellationToken);
                    foreach (var rep in reps.Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: rep.Representative.UserId,
                            Type: NotificationTypes.Assignment,
                            EventCode: NotificationEventCodes.StudentUnassigned,
                            Title: "Cambio en equipo profesional",
                            Message: $"{profName} dejó de estar asignado a {studentName}.",
                            ActionUrl: "/family"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }

            var response = ProfessionalPersonResponse.MapToResponse(assignment);
            return ApiResponse<ProfessionalPersonResponse>.SuccessResult(response, "Asignacion desactivada exitosamente.");
        }
    }
}
