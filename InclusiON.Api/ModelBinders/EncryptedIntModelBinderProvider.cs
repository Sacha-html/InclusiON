using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InclusiON.Api.ModelBinders;

/// <summary>
/// Registrar globalmente en Program.cs (Insert position 1, después de EncryptedGuidModelBinderProvider).
/// Desencripta automáticamente todos los parámetros int/int? de ruta y query.
/// Valores sin prefijo "ENC:" pasan tal cual (lazy-migration fallback — compatibilidad con tests).
/// </summary>
public class EncryptedIntModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(int) || context.Metadata.ModelType == typeof(int?))
            return new EncryptedIntModelBinder();

        return null;
    }
}
