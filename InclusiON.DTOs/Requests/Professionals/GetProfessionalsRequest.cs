using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Professionals
{
    /// <summary>
    /// Request para listar profesionales con filtros y paginacion.
    /// </summary>
    public class GetProfessionalsRequest : PagedRequest
    {
        /// <summary>
        /// Filtro por nombre, apellido o documento (busqueda parcial).
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Filtro por especialidad.
        /// </summary>
        public string? Specialty { get; set; }

        /// <summary>
        /// Filtro por estado activo (basado en User.IsActive).
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filtro por institucion educativa.
        /// </summary>
        public int? InstitutionId { get; set; }
    }
}
