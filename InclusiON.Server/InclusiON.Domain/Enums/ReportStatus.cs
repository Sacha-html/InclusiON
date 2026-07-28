namespace InclusiON.Domain.Enums
{
    /// <summary>
    /// Estado del reporte en el flujo de revisión.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Borrador: creado por el profesional, aún no enviado.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Enviado: el profesional lo remitió al admin para revisión.
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Aprobado: el admin lo aprobó. El familiar puede consultarlo.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rechazado: el admin lo rechazó con un comentario para el profesional.
        /// </summary>
        Rejected = 3
    }
}
