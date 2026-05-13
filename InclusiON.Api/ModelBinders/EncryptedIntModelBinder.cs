using Microsoft.AspNetCore.Mvc.ModelBinding;
using InclusiON.Api.Converters;
using InclusiON.Data.Converters;

namespace InclusiON.Api.ModelBinders;

/// <summary>
/// Desencripta un ID entero desde la URL.
/// El valor en la ruta debe ser un string con formato ENC:&lt;base64url&gt;
/// generado por IEncryptionService.Encrypt(id.ToString()).
/// Usar como: [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId
/// </summary>
public class EncryptedIntModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var raw = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (string.IsNullOrEmpty(raw))
        {
            // Not provided — let framework apply default/null for optional params.
            // Required route params are always present when route matches.
            return Task.CompletedTask;
        }

        try
        {
            // Only apply base64url → standard base64 conversion for encrypted values.
            // Plain integers passed by integration tests must reach Decrypt as-is so
            // the passthrough fallback ("no ENC: prefix → return as-is") works correctly.
            // ToStandardBase64 adds "=" padding that breaks int.TryParse on plain values.
            var toDecrypt = raw.StartsWith("ENC:", StringComparison.Ordinal)
                ? EncryptedGuidConverter.ToStandardBase64(raw)
                : raw;
            var decrypted = EncryptionAccessor.Decrypt(toDecrypt);

            if (!int.TryParse(decrypted, out var id))
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Identificador inválido.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(id);
        }
        catch
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Identificador inválido.");
        }

        return Task.CompletedTask;
    }
}
