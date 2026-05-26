using System.Text.Json;
using System.Text.Json.Serialization;
using InclusiON.Data.Converters;

namespace InclusiON.Api.Converters;

// Encripta Guids al serializar y los desencripta al deserializar.
// Usa Base64Url (sin +, /, ni =) para que los IDs puedan usarse como segmentos de URL
// sin que el / del Base64 estándar rompa el path.
public class EncryptedGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.GetString();
        if (string.IsNullOrEmpty(token))
            return Guid.Empty;

        var decrypted = EncryptionAccessor.Decrypt(ToStandardBase64(token));
        return Guid.Parse(decrypted);
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        => writer.WriteStringValue(ToUrlSafeBase64(EncryptionAccessor.Encrypt(value.ToString())));

    internal static string ToUrlSafeBase64(string standard)
        => standard.Replace('+', '-').Replace('/', '_').TrimEnd('=');

    internal static string ToStandardBase64(string urlSafe)
    {
        var s = urlSafe.Replace('-', '+').Replace('_', '/');
        return (s.Length % 4) switch
        {
            2 => s + "==",
            3 => s + "=",
            _ => s
        };
    }
}

public class EncryptedNullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var token = reader.GetString();
        if (string.IsNullOrEmpty(token))
            return null;

        var decrypted = EncryptionAccessor.Decrypt(EncryptedGuidConverter.ToStandardBase64(token));
        return Guid.Parse(decrypted);
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(EncryptedGuidConverter.ToUrlSafeBase64(EncryptionAccessor.Encrypt(value.Value.ToString())));
    }
}
