using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Persons.Handlers;

/// <summary>
/// Notifica en tiempo real (y persiste en Notifications) a tutores y profesionales supervisores
/// cuando una persona solicita ayuda.
/// </summary>
public class RequestHelpCommandHandler
    : ICommandHandler<RequestHelpCommand, ApiResponse<object>>
{
    private readonly IPersonsRepository _personsRepository;
    private readonly IRealTimeNotifier _notifier;
    private readonly ILogger<RequestHelpCommandHandler> _logger;
    private readonly INotificationService? _notificationService;

    public RequestHelpCommandHandler(
        IPersonsRepository personsRepository,
        IRealTimeNotifier notifier,
        ILogger<RequestHelpCommandHandler> logger,
        INotificationService? notificationService = null)
    {
        _personsRepository   = personsRepository;
        _notifier            = notifier;
        _logger              = logger;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<object>> HandleAsync(
        RequestHelpCommand command, CancellationToken cancellationToken)
    {
        var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
        if (person is null)
            return ApiResponse<object>.NotFound("Persona");

        var personName = $"{person.FirstName} {person.LastName}".Trim();
        var notifications = new List<NotificationMessage>();

        // 1. Obtener representantes familiares activos
        var representatives = await _personsRepository
            .GetActiveRepresentativesAsync(command.PersonId, cancellationToken);

        foreach (var rep in representatives)
        {
            if (rep.Representative != null && rep.Representative.UserId != Guid.Empty)
            {
                notifications.Add(new NotificationMessage(
                    UserId: rep.Representative.UserId,
                    Type: NotificationTypes.Alert,
                    EventCode: "person.help_requested",
                    Title: "Solicitud de ayuda",
                    Message: $"{personName} necesita ayuda urgente.",
                    ActionUrl: "/family"));
            }
        }

        // 2. Si el alumno tiene ingreso asistido (LoginMethodId == 3), también se notifica al profesional supervisor
        if (person.LoginMethodId == 3)
        {
            var profUrl = _notificationService is not null
                ? $"/pro/persons/{_notificationService.EncryptId(command.PersonId)}"
                : $"/pro/persons/{command.PersonId}";

            var supervisors = await _personsRepository
                .GetSupervisingProfessionalsAsync(command.PersonId, cancellationToken);

            var notifiedProfUserIds = new HashSet<Guid>();

            foreach (var prof in supervisors)
            {
                if (prof.UserId != Guid.Empty && notifiedProfUserIds.Add(prof.UserId))
                {
                    notifications.Add(new NotificationMessage(
                        UserId: prof.UserId,
                        Type: NotificationTypes.Alert,
                        EventCode: "person.help_requested",
                        Title: "Solicitud de ayuda",
                        Message: $"{personName} necesita ayuda urgente.",
                        ActionUrl: profUrl));
                }
            }

            if (person.SupervisorUserId.HasValue && person.SupervisorUserId.Value != Guid.Empty && notifiedProfUserIds.Add(person.SupervisorUserId.Value))
            {
                notifications.Add(new NotificationMessage(
                    UserId: person.SupervisorUserId.Value,
                    Type: NotificationTypes.Alert,
                    EventCode: "person.help_requested",
                    Title: "Solicitud de ayuda",
                    Message: $"{personName} necesita ayuda urgente.",
                    ActionUrl: profUrl));
            }
        }

        if (_notificationService is not null)
        {
            await _notificationService.NotifyManyAsync(notifications, cancellationToken);
        }
        else if (notifications.Count > 0)
        {
            await Task.WhenAll(notifications.Select(n =>
                _notifier.NotifyUserAsync(n.UserId.ToString(), n.Title, n.Message, n.ActionUrl, cancellationToken)));
        }

        _logger.LogInformation(
            "Solicitud de ayuda de {PersonId} procesada. Notificados: {Count} destinos.",
            command.PersonId, notifications.Count);

        return ApiResponse<object>.SuccessResult("Solicitud enviada.");
    }
}
