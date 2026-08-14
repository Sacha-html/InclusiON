using System;
using System.Collections.Generic;

namespace InclusiON.DTOs.Responses.Analytics
{
    public class AdminReportsAnalyticsResponse
    {
        public int Pendientes_Revision { get; set; }
        public decimal Tasa_Rechazo { get; set; }
        public decimal Indice_Lectura_Familiar { get; set; }
        public int Total_Reportes { get; set; }
        public int Aprobados_Total { get; set; }
        public int Borradores_Total { get; set; }
        public int Rechazados_Total { get; set; }

        public List<ProfessionalReportsProductivityItem> Ranking_Profesionales { get; set; } = new();
        public List<ReportStatusDistributionItem> Distribucion_Estados { get; set; } = new();
    }

    public class ProfessionalReportsProductivityItem
    {
        public Guid ProfessionalId { get; set; }
        public string NombreProfesional { get; set; } = string.Empty;
        public int ReportesAprobados { get; set; }
        public int TotalReportes { get; set; }
    }

    public class ReportStatusDistributionItem
    {
        public string Estado { get; set; } = string.Empty;
        public string EstadoKey { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
