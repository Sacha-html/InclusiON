namespace InclusiON.DTOs.Common
{
    /// <summary>
    /// Interfaz para request DTOs que soportan filtrado por institucion.
    /// El InstitutionAccessFilter usa esta interfaz para aplicar enforcement
    /// de acceso por institucion a admins no-globales.
    /// </summary>
    public interface IInstitutionFilterable
    {
        /// <summary>
        /// ID de institucion enviado por el cliente (model binding).
        /// </summary>
        int? InstitutionId { get; set; }

        /// <summary>
        /// IDs de instituciones validados por el filter.
        /// Los queries y repos deben usar esta propiedad para filtrar.
        /// </summary>
        List<int>? InstitutionIds { get; set; }
    }
}
