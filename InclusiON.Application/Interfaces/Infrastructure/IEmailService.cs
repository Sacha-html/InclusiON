namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
        Task<bool> SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string?> replacements, CancellationToken cancellationToken = default);
    }
}
