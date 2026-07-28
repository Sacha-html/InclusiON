namespace InclusiON.Application.Auditing
{
    /// <summary>
    /// Registro inmutable de un evento de auditoria de acceso a datos sensibles.
    /// Capturado al momento de la verificacion de autorizacion por recurso.
    /// </summary>
    public sealed record AccessAuditEntry
    {
        /// <summary>
        /// Usuario que realizo el intento de acceso.
        /// </summary>
        public required Guid UserId { get; init; }

        /// <summary>
        /// Rol del usuario al momento del acceso (del claim del JWT).
        /// </summary>
        public string? Role { get; init; }

        /// <summary>
        /// Id de la persona cuyos datos fueron accedidos. Puede ser null si el recurso
        /// no esta asociado a una persona (ej. consulta de otro profesional).
        /// </summary>
        public Guid? AccessedPersonId { get; init; }

        /// <summary>
        /// Tipo de accion realizada. Usar <see cref="Domain.Models.AccessAuditValues.Action"/>.
        /// </summary>
        public required string ActionType { get; init; }

        /// <summary>
        /// Resultado de la verificacion de autorizacion. Usar <see cref="Domain.Models.AccessAuditValues.Result"/>.
        /// </summary>
        public required string Result { get; init; }

        /// <summary>
        /// Nombre de la tabla/entidad del recurso consultado (ej. "Diagnoses", "Reports").
        /// </summary>
        public string? AffectedTable { get; init; }

        /// <summary>
        /// Id del registro afectado (stringificado para soportar Guid, int, etc).
        /// </summary>
        public string? AffectedRecordId { get; init; }

        /// <summary>
        /// Detalles adicionales en formato JSON (motivo de denegacion, contexto, etc).
        /// </summary>
        public string? Details { get; init; }
    }
}
