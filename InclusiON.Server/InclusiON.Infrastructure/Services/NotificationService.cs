using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services;

/// <summary>
/// Servicio centralizado de notificaciones: persiste en DB (tabla Notifications)
/// y despacha push en tiempo real vía SignalR una sola vez por destinatario.
/// Garantiza entrega offline, aislamiento estricto por UserId y cumplimiento de IN-217.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _repository;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IEncryptionService _encryption;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationsRepository repository,
        IRealTimeNotifier realTimeNotifier,
        IEncryptionService encryption,
        IDateTimeProvider dateTime,
        ILogger<NotificationService> logger)
    {
        _repository       = repository;
        _realTimeNotifier = realTimeNotifier;
        _encryption       = encryption;
        _dateTime         = dateTime;
        _logger           = logger;
    }

    public Task NotifyAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        => NotifyManyAsync([notification], cancellationToken);

    public async Task NotifyManyAsync(IEnumerable<NotificationMessage> notifications, CancellationToken cancellationToken = default)
    {
        var candidates = notifications
            .Where(n => n.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(n.Title))
            .DistinctBy(n => (n.UserId, n.EventCode, n.Title, n.Message))
            .ToList();

        if (candidates.Count == 0) return;

        try
        {
            // Regla IN-217: Bloqueo server-side de notificaciones de calendario para rol Administrador
            var calendarCandidates = candidates
                .Where(n => string.Equals(n.Type, NotificationTypes.Calendar, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (calendarCandidates.Count > 0)
            {
                var adminIds = await _repository.GetUserIdsInRoleAsync(
                    calendarCandidates.Select(c => c.UserId),
                    "ADMIN",
                    cancellationToken);

                if (adminIds.Count > 0)
                {
                    candidates = candidates
                        .Where(n => !(string.Equals(n.Type, NotificationTypes.Calendar, StringComparison.OrdinalIgnoreCase)
                                      && adminIds.Contains(n.UserId)))
                        .ToList();
                }
            }

            if (candidates.Count == 0) return;

            var now = _dateTime.UtcNow;
            var entities = candidates.Select(n => new Notification
            {
                Id          = Guid.NewGuid(),
                UserId      = n.UserId,
                Type        = string.IsNullOrWhiteSpace(n.Type) ? NotificationTypes.System : n.Type.Trim().ToLowerInvariant(),
                EventCode   = Truncate(n.EventCode, 64),
                Title       = Truncate(n.Title.Trim(), 200),
                Message     = Truncate(n.Message.Trim(), 1000),
                ActionUrl   = string.IsNullOrWhiteSpace(n.ActionUrl) ? null : Truncate(n.ActionUrl.Trim(), 500),
                IsRead      = false,
                IsDismissed = false,
                CreatedAt   = now
            }).ToList();

            // 1. Persistir primero para garantizar entrega offline
            await _repository.AddRangeAndSaveAsync(entities, cancellationToken);

            // 2. Despachar push SignalR en tiempo real (best-effort, una sola vez)
            foreach (var entity in entities)
            {
                try
                {
                    var push = new NotificationPushPayload(
                        Id: EncryptId(entity.Id),
                        Type: entity.Type,
                        EventCode: entity.EventCode,
                        Title: entity.Title,
                        Message: entity.Message,
                        ActionUrl: entity.ActionUrl,
                        IsRead: entity.IsRead,
                        CreatedAt: entity.CreatedAt);

                    await _realTimeNotifier.SendNotificationAsync(entity.UserId.ToString(), push, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Notificación {NotificationId} persistida para {UserId}, pero falló el envío SignalR en tiempo real.",
                        entity.Id, entity.UserId);
                }
            }
        }
        catch (Exception ex)
        {
            // No romper la transacción principal ya confirmada; registrar para diagnóstico
            _logger.LogError(ex, "Error al persistir/despachar lote de {Count} notificaciones.", candidates.Count);
        }
    }

    public string EncryptId(Guid id) => ToUrlSafeBase64(_encryption.Encrypt(id.ToString()));

    public string EncryptIntId(int id) => ToUrlSafeBase64(_encryption.Encrypt(id.ToString()));

    private static string ToUrlSafeBase64(string standard)
        => standard.Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Truncate(string value, int maxLength)
        => string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
}
