using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Agents;

public class NotificationAgent : IJobHandler
{
    readonly IEmailService _emailService;
    readonly ILogger<NotificationAgent> _logger;

    public int JobTypeId => JobTypes.Push;

    public NotificationAgent(IEmailService emailService, ILogger<NotificationAgent> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationPayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid notification payload");

        _logger.LogInformation("Notification for user {UserId}: {Title}", payload.UserId, payload.Title);

        if (payload.SendEmailFallback && payload.Email is not null)
        {
            var sent = await _emailService.SendTemplatedEmailAsync(
                payload.Email, payload.Title, "notification",
                new Dictionary<string, string?>
                {
                    ["{{Title}}"] = payload.Title,
                    ["{{Message}}"] = payload.Message,
                    ["{{ActionUrl}}"] = payload.ActionUrl
                }, cancellationToken);

            if (sent)
                _logger.LogInformation("Email fallback sent to {Email}", payload.Email);
        }
    }
}

public record NotificationPayload
{
    public string UserId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public string? Email { get; init; }
    public bool SendEmailFallback { get; init; } = true;
}
