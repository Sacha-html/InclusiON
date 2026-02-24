using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catalogo de tipos de reportes.
    /// Define los diferentes formatos y propositos de reportes generados.
    /// </summary>
    public class ReportType : NameableEntity
    {
        /// <summary>
        /// Descripcion del proposito y contenido del tipo de reporte.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si el tipo esta activo para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Reportes generados de este tipo.
        /// </summary>
        public virtual ICollection<Report> Reports { get; set; }

        public ReportType()
        {
            Reports = new HashSet<Report>();
        }
    }
}
