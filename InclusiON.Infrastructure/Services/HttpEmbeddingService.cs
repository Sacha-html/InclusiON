using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Responses.Agents;

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
            throw new InvalidOperationException("Python agent returned empty vector for query");

        return result.Vector;
    }
}

file sealed record EmbedRequest
{
    [JsonPropertyName("entity_type")]
    public string EntityType { get; init; } = string.Empty;
    [JsonPropertyName("entity_id")]
    public string EntityId { get; init; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
    [JsonPropertyName("instructions")]
    public string? Instructions { get; init; }
    [JsonPropertyName("content_json")]
    public string? ContentJson { get; init; }
}


