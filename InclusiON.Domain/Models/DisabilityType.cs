using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catalogo de tipos de discapacidad.
    /// Clasificacion segun CIF (Clasificacion Internacional del Funcionamiento).
    /// </summary>
    public class DisabilityType : NameableEntity, IActivatable
    {
        /// <summary>
        /// Descripcion detallada del tipo de discapacidad.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si el tipo esta activo para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Personas con discapacidad que tienen este tipo.
        /// </summary>
        public virtual ICollection<PersonWithDisability> PersonsWithDisability { get; set; }

        public DisabilityType()
        {
            PersonsWithDisability = new HashSet<PersonWithDisability>();
        }
    }
}
