using InclusiON.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;

namespace InclusiON.SemanticSearch.Services;

/// <summary>
/// Fallback cuando el modelo ONNX no puede cargarse (archivo ausente o formato incorrecto).
/// Devuelve vector vacío — búsqueda semántica no funciona pero el resto de la app sí.
/// </summary>
public sealed class NullEmbeddingService : IEmbeddingService
{
    private readonly ILogger<NullEmbeddingService> _logger;

    public NullEmbeddingService(ILogger<NullEmbeddingService> logger)
    {
        _logger = logger;
        _logger.LogWarning("NullEmbeddingService activo — búsqueda semántica deshabilitada. Verificar modelo ONNX y SentencePiece.");
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(Array.Empty<float>());
}
