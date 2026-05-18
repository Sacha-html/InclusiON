using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

public class EmailAgent(IEmailService emailService, ILogger<EmailAgent> logger) : IJobHandler
{
    public int JobTypeId => JobTypes.Email;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<EmailPayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid email payload");

        if (payload.TemplateName is not null)
        {
            var sent = await emailService.SendTemplatedEmailAsync(
                payload.To, payload.Subject, payload.TemplateName,
                payload.Replacements ?? [], cancellationToken);

            if (!sent)
                throw new InvalidOperationException($"Failed to send templated email '{payload.TemplateName}' to {payload.To}");
        }
        else
        {
            var sent = await emailService.SendEmailAsync(
                payload.To, payload.Subject,
                payload.HtmlBody ?? string.Empty, cancellationToken);

            if (!sent)
                throw new InvalidOperationException($"Failed to send email to {payload.To}");
        }

        logger.LogInformation("Email sent to {To}, subject '{Subject}'", payload.To, payload.Subject);
    }
}

public record EmailPayload
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, string?>? Replacements { get; init; }
}
