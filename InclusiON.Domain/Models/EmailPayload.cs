namespace InclusiON.Domain.Models;

public record EmailPayload
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, string?>? Replacements { get; init; }
}
