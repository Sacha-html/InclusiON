using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Responses.Agents;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

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

        switch (payload.EntityType)
        {
            case "activity":
                await embeddingRepository.StoreAsync(int.Parse(payload.EntityId), result.Vector, cancellationToken);
                break;
            case "person":
                await embeddingRepository.StorePersonAsync(Guid.Parse(payload.EntityId), result.Vector, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unknown entity type for embedding: '{payload.EntityType}'");
        }

        logger.LogInformation("Stored embedding for {EntityType} {EntityId} ({Dimensions} dims)",
            payload.EntityType, payload.EntityId, result.Vector.Length);
    }
}

file sealed record EmbeddingPayload
{
    [JsonPropertyName("entity_type")]
    public string EntityType { get; init; } = string.Empty;
    [JsonPropertyName("entity_id")]
    public string EntityId { get; init; } = string.Empty;
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("instructions")]
    public string? Instructions { get; init; }
    [JsonPropertyName("content_json")]
    public string? ContentJson { get; init; }
}


