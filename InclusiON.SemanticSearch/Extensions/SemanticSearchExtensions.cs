using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.SemanticSearch.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InclusiON.SemanticSearch.Extensions;

public static class SemanticSearchExtensions
{
    public static IServiceCollection AddSemanticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var modelPath = configuration["SemanticSearch:ModelPath"]
            ?? throw new InvalidOperationException("SemanticSearch:ModelPath no configurado.");

        var spmPath = configuration["SemanticSearch:SentencePieceModelPath"]
            ?? throw new InvalidOperationException("SemanticSearch:SentencePieceModelPath no configurado.");

        static string Resolve(string path) =>
            Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

        var resolvedModelPath = Resolve(modelPath);
        var resolvedSpmPath   = Resolve(spmPath);

        // Singleton: carga el modelo ONNX y el tokenizador una sola vez.
        // Si el modelo no está disponible o el archivo es incompatible, cae a NullEmbeddingService
        // para que la app funcione sin búsqueda semántica.
        services.AddSingleton<IEmbeddingService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OnnxEmbeddingService>>();
            try
            {
                return new OnnxEmbeddingService(resolvedModelPath, resolvedSpmPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo cargar el modelo ONNX desde '{ModelPath}' / '{SpmPath}'. Búsqueda semántica deshabilitada.", resolvedModelPath, resolvedSpmPath);
                var nullLogger = sp.GetRequiredService<ILogger<NullEmbeddingService>>();
                return new NullEmbeddingService(nullLogger);
            }
        });

        return services;
    }
}
