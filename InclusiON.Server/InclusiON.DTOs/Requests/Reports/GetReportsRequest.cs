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
        public List<string>? PersonIds { get; set; }
        public string? ProfessionalId { get; set; }
        public string? ReportTypeId { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>Filtro por estado del flujo (Draft, Submitted, Approved, Rejected).</summary>
        public string? Status { get; set; }

        /// <summary>Filtro desde fecha de reporte (inclusive).</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>Alias en español para DateFrom.</summary>
        public DateTime? Desde
        {
            get => DateFrom;
            set => DateFrom ??= value;
        }

        /// <summary>Filtro hasta fecha de reporte (inclusive).</summary>
        public DateTime? DateTo { get; set; }

        /// <summary>Alias en español para DateTo.</summary>
        public DateTime? Hasta
        {
            get => DateTo;
            set => DateTo ??= value;
        }

        public int? InstitutionId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? InstitutionIds { get; set; }

        // Las columnas de esta lista (persona, profesional, tipo) no forman parte del
        // enum SortField compartido: bindear SortBy contra ese enum hacia fallar el
        // model binding (400) apenas se ordenaba por cualquier columna que no fuera
        // Title/ReportDate/CreatedAt. Se ocultan las propiedades de la base y se usan
        // como texto libre (mismo criterio que GetActivitiesRequest, INCNEW-36).
        public new string? SortBy { get; set; }
        public new string SortDirection { get; set; } = "ASC";
    }
}
