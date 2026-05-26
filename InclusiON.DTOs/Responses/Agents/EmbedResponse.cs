using System.Text.Json.Serialization;

namespace InclusiON.DTOs.Responses.Agents;

public sealed record EmbedResponse
{
    [JsonPropertyName("vector")]
    public float[]? Vector { get; init; }
}
