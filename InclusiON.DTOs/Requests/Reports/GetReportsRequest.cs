using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Reports
{
    /// <summary>
    /// Request para listar reportes con filtros y paginación.
    /// </summary>
    public class GetReportsRequest : PagedRequest, IInstitutionFilterable
    {
        public string? Search { get; set; }
        public string? PersonId { get; set; }
        public string? ProfessionalId { get; set; }
        public string? ReportTypeId { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>Filtro por estado del flujo (Draft, Submitted, Approved, Rejected).</summary>
        public string? Status { get; set; }

        /// <summary>Filtro desde fecha de reporte (inclusive).</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>Filtro hasta fecha de reporte (inclusive).</summary>
        public DateTime? DateTo { get; set; }

        public int? InstitutionId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? InstitutionIds { get; set; }
    }
}
