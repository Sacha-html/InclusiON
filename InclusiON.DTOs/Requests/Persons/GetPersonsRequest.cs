using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para listar personas con discapacidad con filtros y paginacion.
    /// </summary>
    public class GetPersonsRequest : PagedRequest
    {
        /// <summary>
        /// Filtro por nombre o apellido (busqueda parcial).
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Filtro por tipo de discapacidad.
        /// </summary>
        public int? DisabilityTypeId { get; set; }

        /// <summary>
        /// Filtro por nivel de autonomia.
        /// </summary>
        public int? AutonomyLevelId { get; set; }

        /// <summary>
        /// Filtro por estado activo (basado en User.IsActive).
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
