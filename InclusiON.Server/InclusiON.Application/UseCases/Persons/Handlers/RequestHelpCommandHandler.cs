using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Persons.Handlers;

/// <summary>
/// Notifica en tiempo real (SignalR directo, sin pasar por cola de jobs)
/// a todos los profesionales supervisores de la persona.
/// Latencia ~0 vs. hasta 60s con el job dispatcher.
/// </summary>
public class RequestHelpCommandHandler
    : ICommandHandler<RequestHelpCommand, ApiResponse<object>>
{
    private readonly IPersonsRepository  _personsRepository;
    private readonly IRealTimeNotifier   _notifier;
    private readonly ILogger<RequestHelpCommandHandler> _logger;

    public RequestHelpCommandHandler(
        IPersonsRepository personsRepository,
        IRealTimeNotifier notifier,
        ILogger<RequestHelpCommandHandler> logger)
    {
        _personsRepository = personsRepository;
        _notifier          = notifier;
        _logger            = logger;
    }

    public async Task<ApiResponse<object>> HandleAsync(
        RequestHelpCommand command, CancellationToken cancellationToken)
    {
        var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
        if (person is null)
            return ApiResponse<object>.NotFound("Persona");

        var personName = $"{person.FirstName} {person.LastName}";

        // 1. Obtener representantes familiares activos
        var representatives = await _personsRepository
            .GetActiveRepresentativesAsync(command.PersonId, cancellationToken);

        var tasks = new List<Task>();

        // Notificar siempre a los tutores/familiares vinculados
        foreach (var rep in representatives)
        {
            if (rep.Representative != null && rep.Representative.UserId != Guid.Empty)
            {
                tasks.Add(_notifier.NotifyUserAsync(
                    rep.Representative.UserId.ToString(),
                    "🆘 Solicitud de ayuda",
                    $"{personName} necesita ayuda urgente.",
                    actionUrl: "/#/family/dashboard",
                    cancellationToken: cancellationToken));
            }
        }

        // 2. Si el alumno tiene ingreso asistido (LoginMethodId == 3), también se notifica al profesional supervisor
        if (person.LoginMethodId == 3)
        {
            var supervisors = await _personsRepository
                .GetSupervisingProfessionalsAsync(command.PersonId, cancellationToken);

            var notifiedProfUserIds = new HashSet<string>();

            foreach (var prof in supervisors)
            {
                var profUserId = prof.UserId.ToString();
                if (notifiedProfUserIds.Add(profUserId))
                {
                    tasks.Add(_notifier.NotifyUserAsync(
                        profUserId,
                        "🆘 Solicitud de ayuda",
                        $"{personName} necesita ayuda urgente.",
                        actionUrl: $"/#/pro/persons/{command.PersonId}",
                        cancellationToken: cancellationToken));
                }
            }

            // Si hay un supervisor específico guardado en SupervisorUserId
            if (person.SupervisorUserId.HasValue)
            {
                var specificSupervisorUserIdStr = person.SupervisorUserId.Value.ToString();
                if (notifiedProfUserIds.Add(specificSupervisorUserIdStr))
                {
                    tasks.Add(_notifier.NotifyUserAsync(
                        specificSupervisorUserIdStr,
                        "🆘 Solicitud de ayuda",
                        $"{personName} necesita ayuda urgente.",
                        actionUrl: $"/#/pro/persons/{command.PersonId}",
                        cancellationToken: cancellationToken));
                }
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        _logger.LogInformation(
            "Solicitud de ayuda de {PersonId} procesada. Notificados: {Count} destinos.",
            command.PersonId, tasks.Count);

        return ApiResponse<object>.SuccessResult("Solicitud enviada.");
    }
}
