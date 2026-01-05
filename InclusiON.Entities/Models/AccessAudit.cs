namespace InclusiON.Entities.Models
{
    /// <summary>
    /// Registro de auditoria de acceso a datos sensibles.
    /// Documenta quien accedio a que informacion y cuando, para cumplimiento de privacidad.
    /// </summary>
    public class AccessAudit
    {
        /// <summary>
        /// Identificador unico del registro de auditoria.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que realizo la accion.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID de la persona cuyos datos fueron accedidos (si aplica).
        /// </summary>
        public Guid? AccessedPersonId { get; set; }

        /// <summary>
        /// Tipo de accion realizada (Lectura, Creacion, Modificacion, Eliminacion).
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la tabla afectada.
        /// </summary>
        public string? AffectedTable { get; set; }

        /// <summary>
        /// ID del registro afectado.
        /// </summary>
        public string? AffectedRecordId { get; set; }

        /// <summary>
        /// Direccion IP desde donde se realizo la accion.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Fecha y hora de la accion.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Detalles adicionales de la accion en formato JSON.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Usuario que realizo la accion.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Persona cuyos datos fueron accedidos.
        /// </summary>
        public virtual PersonWithDisability? AccessedPerson { get; set; }
    }
}
