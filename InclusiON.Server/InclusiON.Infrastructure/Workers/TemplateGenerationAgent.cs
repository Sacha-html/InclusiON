using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

/// <summary>
/// Regenera los embeddings de todas las actividades estándar (IsStandardActivity = true).
/// Se dispara cada noche desde GenerateTemplateCentroidsStep vía MidnightCleanupWorker.
/// Encola un job JobTypes.Embedding por cada actividad estándar activa para que
/// EmbeddingAgent los procese en los ciclos siguientes del PendingJobsWorker.
/// </summary>
public class TemplateGenerationAgent(
    IActivitiesRepository activitiesRepository,
    IBackgroundJobRepository backgroundJobs,
    ILogger<TemplateGenerationAgent> logger)
    : IJobHandler
{
    public int JobTypeId => JobTypes.TemplateGeneration;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var activities = await activitiesRepository.GetStandardActivitiesForEmbeddingAsync(cancellationToken);

        if (activities.Count == 0)
        {
            logger.LogInformation("TemplateGenerationAgent: no standard activities found, skipping");
            return;
        }

        logger.LogInformation("TemplateGenerationAgent: enqueuing embedding refresh for {Count} standard activities", activities.Count);

        var enqueued = 0;
        foreach (var activity in activities)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new EmbeddingPayload
                {
                    EntityType   = "activity",
                    EntityId     = activity.Id.ToString(),
                    Title        = activity.Title,
                    Description  = activity.Description,
                    Instructions = activity.Instructions,
                    ContentJson  = activity.ContentJson,
                });

                await backgroundJobs.CreateAsync(
                    JobTypes.Embedding,
                    payload,
                    scheduledAt: DateTime.UtcNow,
                    maxRetries: 2,
                    cancellationToken: cancellationToken);

                enqueued++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "TemplateGenerationAgent: failed to enqueue embedding job for activity {ActivityId}", activity.Id);
            }
        }

        logger.LogInformation("TemplateGenerationAgent: enqueued {Enqueued}/{Total} embedding jobs", enqueued, activities.Count);
    }
}

file sealed record EmbeddingPayload
{
    [System.Text.Json.Serialization.JsonPropertyName("entity_type")]
    public string EntityType { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("entity_id")]
    public string EntityId { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("instructions")]
    public string? Instructions { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("content_json")]
    public string? ContentJson { get; init; }
}
