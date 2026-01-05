using InclusiON.Entities.Models.BaseEntities;

namespace InclusiON.Entities.Models
{
    /// <summary>
    /// Institucion educativa donde trabajan los profesionales.
    /// Puede ser escuela, centro de rehabilitacion, consultorio, etc.
    /// </summary>
    public class EducationalInstitution : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la institucion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la institucion educativa.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Direccion fisica de la institucion.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Numero de telefono de contacto.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Correo electronico de contacto.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Profesionales que trabajan en esta institucion.
        /// </summary>
        public virtual ICollection<ProfessionalInstitution> ProfessionalInstitutions { get; set; }

        public EducationalInstitution()
        {
            ProfessionalInstitutions = new HashSet<ProfessionalInstitution>();
        }
    }
}
