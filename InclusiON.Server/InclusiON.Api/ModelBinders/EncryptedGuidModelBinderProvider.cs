using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InclusiON.Api.ModelBinders;

public class EncryptedGuidModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(Guid) || context.Metadata.ModelType == typeof(Guid?))
            return new EncryptedGuidModelBinder();

        return null;
    }
}
