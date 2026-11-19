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
    public class DeactivatePersonCommandHandler : ICommandHandler<DeactivatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivatePersonCommandHandler> _logger;
        private readonly IAssignmentsRepository? _assignmentsRepository;
        private readonly IFamilyRepository? _familyRepository;
        private readonly INotificationService? _notificationService;

        public DeactivatePersonCommandHandler(
            IPersonsRepository repository,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivatePersonCommandHandler> logger,
            IAssignmentsRepository? assignmentsRepository = null,
            IFamilyRepository? familyRepository = null,
            INotificationService? notificationService = null)
        {
            _repository              = repository;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork              = unitOfWork;
            _logger                  = logger;
            _assignmentsRepository   = assignmentsRepository;
            _familyRepository        = familyRepository;
            _notificationService     = notificationService;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(DeactivatePersonCommand command, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Desactivar persona y usuario
            person.IsActive = false;
            person.User.IsActive = false;

            // Revocar refresh tokens activos
            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                person.UserId,
                "Persona desactivada",
                cancellationToken);

            await _repository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persona desactivada: {PersonId}, Usuario: {UserId}", command.PersonId, person.UserId);

            if (_notificationService is not null)
            {
                var notifications = new List<NotificationMessage>();
                var studentName = $"{person.FirstName} {person.LastName}".Trim();

                if (_assignmentsRepository is not null)
                {
                    var profs = await _assignmentsRepository.GetProfessionalsByPersonIdAsync(person.Id, cancellationToken);
                    foreach (var pp in profs.Where(p => p.IsActive && p.Professional is not null && p.Professional.UserId != Guid.Empty))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: pp.Professional.UserId,
                            Type: NotificationTypes.Alert,
                            EventCode: NotificationEventCodes.StudentDeactivated,
                            Title: "Alumno dado de baja",
                            Message: $"El alumno {studentName} fue dado de baja del sistema.",
                            ActionUrl: "/pro/persons"));
                    }
                }

                if (_familyRepository is not null)
                {
                    var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(person.Id, cancellationToken);
                    foreach (var rep in reps.Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: rep.Representative.UserId,
                            Type: NotificationTypes.Alert,
                            EventCode: NotificationEventCodes.StudentDeactivated,
                            Title: "Cuenta de alumno desactivada",
                            Message: $"La cuenta de {studentName} fue dada de baja en el sistema.",
                            ActionUrl: "/family"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, SuccessMessages.PersonDeactivated);
        }
    }
}
