using System.Net.Http.Json;
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
        var response = await client.PostAsJsonAsync("/embed", new
        {
            entity_type = "query",
            entity_id   = "0",
            title       = text,
            description = string.Empty,
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken);

        if (result?.Vector is null || result.Vector.Length == 0)
            throw new InvalidOperationException("Python agent returned empty vector for query");

        return result.Vector;
    }
}


