using Microsoft.AspNetCore.Mvc.ModelBinding;
using InclusiON.Api.Converters;
using InclusiON.Data.Converters;

namespace InclusiON.Api.ModelBinders;

public class EncryptedGuidModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var raw = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (string.IsNullOrEmpty(raw))
        {
            if (bindingContext.ModelType == typeof(Guid?))
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "El identificador es requerido.");

            return Task.CompletedTask;
        }

        try
        {
            var standard = EncryptedGuidConverter.ToStandardBase64(raw);
            var decrypted = EncryptionAccessor.Decrypt(standard);
            var guid = Guid.Parse(decrypted);
            bindingContext.Result = bindingContext.ModelType == typeof(Guid?)
                ? ModelBindingResult.Success((Guid?)guid)
                : ModelBindingResult.Success(guid);
        }
        catch
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Identificador inválido.");
        }

        return Task.CompletedTask;
    }
}
