using System.Text.Json;
using System.Text.Json.Serialization;

namespace InclusiON.Api.Converters;

/// <summary>
/// Normaliza todos los DateTime que entran por la API a Kind=Utc.
/// Npgsql rechaza Kind=Unspecified en columnas timestamp with time zone.
/// Para fechas puras (BirthDate, ReportDate, etc.) el frontend envía ISO 8601
/// sin zona horaria — este converter los fuerza a UTC antes de que lleguen a EF Core.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) // Unspecified → tratar como UTC
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Serializar siempre como UTC con sufijo Z
        writer.WriteStringValue(value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
