using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Dispositivo de confianza registrado para un usuario.
    /// Permite login automatico sin credenciales para personas con autonomia baja.
    /// Debe ser autorizado por un familiar o profesional.
    /// </summary>
    public class TrustedDevice : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del registro de dispositivo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario propietario del dispositivo.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Identificador unico del dispositivo (fingerprint o UUID).
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre amigable del dispositivo (ej: "Tablet de Juan").
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// Tipo de dispositivo (ej: Tablet, Smartphone, Desktop).
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Navegador utilizado (ej: Chrome, Firefox, Safari).
        /// </summary>
        public string? Browser { get; set; }

        /// <summary>
        /// Sistema operativo del dispositivo (ej: Android, iOS, Windows).
        /// </summary>
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Fecha y hora en que se registro el dispositivo.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Fecha y hora del ultimo uso del dispositivo.
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Fecha de expiracion de la confianza (requiere reautorizacion).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// ID del usuario (familiar/profesional) que autorizo este dispositivo.
        /// </summary>
        public Guid? AuthorizedByUserId { get; set; }

        /// <summary>
        /// Usuario propietario del dispositivo.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Usuario que autorizo este dispositivo como confiable.
        /// </summary>
        public virtual User? AuthorizedByUser { get; set; }
    }
}
