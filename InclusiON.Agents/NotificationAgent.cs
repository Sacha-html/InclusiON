using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Agents;

public class NotificationAgent(
    IRealTimeNotifier notifier,
    IEmailService emailService,
    ILogger<NotificationAgent> logger)
    : IJobHandler
{
    public int JobTypeId => JobTypes.Push;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationPayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid notification payload");

        logger.LogInformation("Notification for user {UserId}: {Title}", payload.UserId, payload.Title);

        await notifier.NotifyUserAsync(payload.UserId, payload.Title, payload.Message, payload.ActionUrl, cancellationToken);

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
}
