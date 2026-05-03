using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.SemanticSearch.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InclusiON.SemanticSearch.Extensions;

public static class SemanticSearchExtensions
{
    public static IServiceCollection AddSemanticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var modelPath = configuration["SemanticSearch:ModelPath"]
            ?? throw new InvalidOperationException("SemanticSearch:ModelPath no configurado.");

        var spmPath = configuration["SemanticSearch:SentencePieceModelPath"]
            ?? throw new InvalidOperationException("SemanticSearch:SentencePieceModelPath no configurado.");

        // Paths relativos se resuelven desde el directorio del binario (donde el .csproj copia Model/).
        // Paths absolutos (configuración de producción custom) se usan tal cual.
        static string Resolve(string path) =>
            Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

        var resolvedModelPath = Resolve(modelPath);
        var resolvedSpmPath   = Resolve(spmPath);

        // Singleton: carga el modelo ONNX (~450MB) y el tokenizador una sola vez al iniciar.
        services.AddSingleton<IEmbeddingService>(_ =>
            new OnnxEmbeddingService(resolvedModelPath, resolvedSpmPath));

        return services;
    }
}
