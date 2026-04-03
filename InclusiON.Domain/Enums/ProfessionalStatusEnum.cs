namespace InclusiON.Domain.Enums
{
    /// <summary>
    /// Estados del profesional en el proceso de validación
    /// </summary>
    public enum ProfessionalStatusEnum
    {
        /// <summary>
        /// Pendiente de validación por administrador
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Aprobado y habilitado para acceder al sistema
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Rechazado por administrador
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Suspendido por inactividad (no accede al sistema por mucho tiempo)
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// Dado de baja por administrador
        /// </summary>
        Terminated = 4
    }
}