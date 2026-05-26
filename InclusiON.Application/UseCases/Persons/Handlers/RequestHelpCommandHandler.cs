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

        var supervisors = await _personsRepository
            .GetSupervisingProfessionalsAsync(command.PersonId, cancellationToken);

        if (supervisors.Count == 0)
        {
            _logger.LogWarning(
                "Persona {PersonId} solicitó ayuda pero no tiene profesionales supervisores.", command.PersonId);
            return ApiResponse<object>.SuccessResult("Solicitud enviada.");
        }

        var tasks = supervisors.Select(prof =>
            _notifier.NotifyUserAsync(
                prof.UserId.ToString(),
                "🆘 Solicitud de ayuda",
                $"{personName} necesita ayuda urgente.",
                actionUrl: $"/#/pro/persons/{command.PersonId}",
                cancellationToken: cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Solicitud de ayuda de {PersonId} enviada a {Count} profesionales vía SignalR.",
            command.PersonId, supervisors.Count);

        return ApiResponse<object>.SuccessResult("Solicitud enviada.");
    }
}
