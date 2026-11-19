using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class ReactivatePersonCommandHandler : ICommandHandler<ReactivatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactivatePersonCommandHandler> _logger;
        private readonly IAssignmentsRepository? _assignmentsRepository;
        private readonly IFamilyRepository? _familyRepository;
        private readonly INotificationService? _notificationService;

        public ReactivatePersonCommandHandler(
            IPersonsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ReactivatePersonCommandHandler> logger,
            IAssignmentsRepository? assignmentsRepository = null,
            IFamilyRepository? familyRepository = null,
            INotificationService? notificationService = null)
        {
            _repository            = repository;
            _unitOfWork            = unitOfWork;
            _logger                = logger;
            _assignmentsRepository = assignmentsRepository;
            _familyRepository      = familyRepository;
            _notificationService   = notificationService;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(ReactivatePersonCommand command, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            if (person.IsActive && person.User.IsActive)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    "El alumno ya se encuentra activo");
            }

            // Reactivar persona y usuario
            person.IsActive = true;
            person.User.IsActive = true;

            await _repository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persona reactivada: {PersonId}, Usuario: {UserId}", command.PersonId, person.UserId);

            if (_notificationService is not null)
            {
                var notifications = new List<NotificationMessage>();
                var studentName = $"{person.FirstName} {person.LastName}".Trim();
                var encryptedPersonId = _notificationService.EncryptId(person.Id);

                if (_assignmentsRepository is not null)
                {
                    var profs = await _assignmentsRepository.GetProfessionalsByPersonIdAsync(person.Id, cancellationToken);
                    foreach (var pp in profs.Where(p => p.IsActive && p.Professional is not null && p.Professional.UserId != Guid.Empty))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: pp.Professional.UserId,
                            Type: NotificationTypes.System,
                            EventCode: NotificationEventCodes.StudentReactivated,
                            Title: "Alumno reactivado",
                            Message: $"El alumno {studentName} fue reactivado en el sistema.",
                            ActionUrl: $"/pro/persons/{encryptedPersonId}"));
                    }
                }

                if (_familyRepository is not null)
                {
                    var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(person.Id, cancellationToken);
                    foreach (var rep in reps.Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: rep.Representative.UserId,
                            Type: NotificationTypes.System,
                            EventCode: NotificationEventCodes.StudentReactivated,
                            Title: "Cuenta de alumno reactivada",
                            Message: $"La cuenta de {studentName} fue reactivada en el sistema.",
                            ActionUrl: "/family"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, "Alumno reactivado exitosamente");
        }
    }
}
