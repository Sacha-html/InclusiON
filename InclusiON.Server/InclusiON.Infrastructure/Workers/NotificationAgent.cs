using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

public class NotificationAgent(
    IRealTimeNotifier notifier,
    IEmailService emailService,
    ILogger<NotificationAgent> logger,
    INotificationService? notificationService = null)
    : IJobHandler
{
    public int JobTypeId => JobTypes.Push;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationPayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid notification payload");

        logger.LogInformation("Notification job for user {UserId}: {Title}", payload.UserId, payload.Title);

        if (notificationService is not null && Guid.TryParse(payload.UserId, out var userGuid))
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                UserId: userGuid,
                Type: InferType(payload.Title, payload.ActionUrl),
                EventCode: "legacy.push",
                Title: payload.Title,
                Message: payload.Message,
                ActionUrl: payload.ActionUrl),
                cancellationToken);
        }
        else
        {
            await notifier.NotifyUserAsync(payload.UserId, payload.Title, payload.Message, payload.ActionUrl, cancellationToken);
        }

        if (payload.SendEmailFallback && payload.Email is not null)
        {
            var sent = await emailService.SendTemplatedEmailAsync(
                payload.Email, payload.Title, "notification",
                new Dictionary<string, string?>
                {
                    ["{{Title}}"] = payload.Title,
                    ["{{Message}}"] = payload.Message,
                    ["{{ActionUrl}}"] = payload.ActionUrl
                }, cancellationToken);

            if (sent)
                logger.LogInformation("Email fallback sent to {Email}", payload.Email);
        }
    }

    private static string InferType(string title, string? actionUrl)
    {
        var t = (title ?? string.Empty).ToLowerInvariant();
        var u = (actionUrl ?? string.Empty).ToLowerInvariant();
        if (t.Contains("calendario") || u.Contains("calendar")) return NotificationTypes.Calendar;
        if (t.Contains("mensaje") || t.Contains("respuesta") || u.Contains("messages")) return NotificationTypes.Message;
        if (t.Contains("reporte") || u.Contains("reports")) return NotificationTypes.Report;
        if (t.Contains("alerta") || t.Contains("frustraci") || t.Contains("bloqueada")) return NotificationTypes.Alert;
        if (t.Contains("actividad") || u.Contains("activities") || u.Contains("evaluations")) return NotificationTypes.Activity;
        return NotificationTypes.System;
    }
}
