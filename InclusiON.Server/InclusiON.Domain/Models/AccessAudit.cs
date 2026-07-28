namespace InclusiON.Domain.Models
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
        /// Rol del usuario al momento de la accion (Professional, Family, Admin, GlobalAdmin, Student).
        /// Se captura del claim en tiempo de auditoria para preservar el contexto aunque el rol cambie despues.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// ID de la persona cuyos datos fueron accedidos (si aplica).
        /// </summary>
        public Guid? AccessedPersonId { get; set; }

        /// <summary>
        /// Tipo de accion realizada (Read, Create, Update, Delete).
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Resultado de la verificacion de autorizacion (Allowed, Denied).
        /// Obligatorio: permite auditar tanto accesos concedidos como intentos denegados.
        /// </summary>
        public string Result { get; set; } = string.Empty;

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
        /// Identificador de correlacion (HTTP trace id) para agrupar todas las verificaciones
        /// de autorizacion que ocurren dentro de la misma request.
        /// </summary>
        public string? CorrelationId { get; set; }

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
