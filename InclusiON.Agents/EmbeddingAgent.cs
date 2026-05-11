using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Agents;

public class EmbeddingAgent(
    IHttpClientFactory httpClientFactory,
    IEmbeddingRepository embeddingRepository,
    ILogger<EmbeddingAgent> logger)
    : IJobHandler
{
    public int JobTypeId => JobTypes.Embedding;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<EmbeddingPayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid embedding payload");

        var client = httpClientFactory.CreateClient("PythonAgent");
        var response = await client.PostAsJsonAsync("/embed", new
        {
            entity_type = payload.EntityType,
            entity_id = payload.EntityId,
            title = payload.Title ?? string.Empty,
            description = payload.Description ?? string.Empty,
            instructions = payload.Instructions,
            content_json = payload.ContentJson,
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken);

        if (result?.Vector is null || result.Vector.Length == 0)
            throw new InvalidOperationException("Python agent returned empty vector");

        var activityId = int.Parse(payload.EntityId);
        await embeddingRepository.StoreAsync(activityId, result.Vector, cancellationToken);

        logger.LogInformation("Stored embedding for {EntityType} {EntityId} ({Dimensions} dims)",
            payload.EntityType, payload.EntityId, result.Vector.Length);
    }
}

file sealed record EmbeddingPayload
{
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Instructions { get; init; }
    public string? ContentJson { get; init; }
}

file sealed record EmbedResponse
{
    [JsonPropertyName("vector")]
    public float[]? Vector { get; init; }
}
