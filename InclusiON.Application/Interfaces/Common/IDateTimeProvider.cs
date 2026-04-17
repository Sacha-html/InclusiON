namespace InclusiON.Application.Interfaces.Common
{
    /// <summary>
    /// Abstracción del reloj del sistema. Permite mockear fechas en tests.
    /// Toda referencia a la hora actual debe ir a través de esta interfaz.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Fecha y hora actual en UTC (Kind=Utc). Usar para timestamps de auditoría en la base de datos.
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Fecha y hora actual en la zona horaria de Argentina (America/Argentina/Buenos_Aires, UTC-3).
        /// Usar para lógica de negocio que depende de la fecha/hora local.
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Fecha actual en Argentina (sin hora). Útil para comparaciones de día.
        /// </summary>
        DateOnly Today { get; }
    }
}
