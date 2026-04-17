using InclusiON.Application.Interfaces.Common;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementación del proveedor de fechas usando la zona horaria de Argentina
    /// (America/Argentina/Buenos_Aires, UTC-3, sin horario de verano).
    /// </summary>
    public class ArgentinaDateTimeProvider : IDateTimeProvider
    {
        private static readonly TimeZoneInfo ArgentinaTimeZone = ResolveTimeZone();

        private static TimeZoneInfo ResolveTimeZone()
        {
            // .NET 6+ soporta IDs IANA en Windows si tiene datos ICU actualizados.
            // Fallback al ID nativo de Windows para mayor compatibilidad.
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
        }

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);

        /// <inheritdoc/>
        public DateOnly Today => DateOnly.FromDateTime(Now);
    }
}
