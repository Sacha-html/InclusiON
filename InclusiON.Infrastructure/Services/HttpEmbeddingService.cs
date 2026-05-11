using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Infrastructure.Services;

public class HttpEmbeddingService : IEmbeddingService
{
    readonly IHttpClientFactory _httpClientFactory;
    readonly ILogger<HttpEmbeddingService> _logger;

    public HttpEmbeddingService(IHttpClientFactory httpClientFactory, ILogger<HttpEmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PythonAgent");
            var response = await client.PostAsJsonAsync("/embed", new EmbedRequest
            {
                Title = text,
                Description = string.Empty,
                EntityType = "query",
                EntityId = "0",
            }, cancellationToken);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken);

            if (result?.Vector is null || result.Vector.Length == 0)
            {
                _logger.LogWarning("Python agent returned empty vector for query");
                return [];
            }

            return result.Vector;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding via Python agent");
            return [];
        }
    }
}

file sealed record EmbedRequest
{
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Instructions { get; init; }
    public string? ContentJson { get; init; }
}

file sealed record EmbedResponse
{
    [JsonPropertyName("vector")]
    public float[]? Vector { get; init; }
}
