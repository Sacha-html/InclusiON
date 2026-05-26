namespace InclusiON.Application.UseCases.Persons.Commands;

/// <summary>
/// Solicitud de ayuda urgente enviada por una persona con discapacidad.
/// Notifica vía SignalR a todos sus profesionales supervisores activos.
/// </summary>
public record RequestHelpCommand(Guid PersonId);
