using InclusiON.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InclusiON.DTOs.Requests.Reports
{
    /// <summary>
    /// Request para listar reportes con filtros y paginación.
    /// </summary>
    public class GetReportsRequest : PagedRequest, IInstitutionFilterable
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Filtro por Persona
        /// </summary>
        public string? PersonId { get; set; }

        /// <summary>
        /// Filtro por profesional
        /// </summary>
        public string? ProfessionalId { get; set; }

        /// <summary>
        /// Filtro por Tipo de Reporte
        /// </summary>
        
        public string? ReportTypeId { get; set; }
        /// <summary>
        /// Filtro por institucion educativa.
        /// </summary>

        /// <summary>
        /// Filtro por estado activo (basado en User.IsActive).
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filtro por institucion educativa.
        /// </summary>
        public int? InstitutionId { get; set; }

        /// <summary>
        /// IDs de instituciones validados por el filter (no se bindea desde query string).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? InstitutionIds { get; set; }
    }
}
