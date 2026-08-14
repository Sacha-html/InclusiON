namespace InclusiON.DTOs.Responses.Analytics
{
    /// <summary>
    /// Detalle de una sesión con alerta o bloqueo de frustración para el modal de drill-down.
    /// </summary>
    public class FrustrationDetailResponse
    {
        public Guid StudentId { get; set; }
        public string NombreAlumno { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public string CategoriaPedagogica { get; set; } = string.Empty;
        public int CantidadErrores { get; set; }
        public decimal SuccessRate { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int GasScore { get; set; }
        public DateTime Fecha { get; set; }
        public string MotivoFrustracion { get; set; } = string.Empty;
    }
}
