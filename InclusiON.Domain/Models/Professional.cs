using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Profesional del sistema (docente, terapeuta, psicologo, etc.).
    /// Responsable de crear y gestionar personas con discapacidad, asignar actividades y generar reportes.
    /// </summary>
    public class Professional : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del profesional.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del usuario asociado a este perfil de profesional.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre del profesional.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del profesional.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Numero de documento de identidad.
        /// </summary>
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Numero de telefono de contacto.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Especialidad del profesional (ej: Educacion Especial, Psicologia, Terapia Ocupacional).
        /// </summary>
        public string? Specialty { get; set; }

        /// <summary>
        /// Numero de licencia o matricula profesional.
        /// </summary>
        public string? LicenseNumber { get; set; }

        /// <summary>
        /// Fecha de nacimiento del profesional.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Direccion del profesional.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Usuario asociado a este perfil.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Instituciones educativas donde trabaja el profesional.
        /// </summary>
        public virtual ICollection<ProfessionalInstitution> ProfessionalInstitutions { get; set; }

        /// <summary>
        /// Personas con discapacidad asignadas a este profesional.
        /// </summary>
        public virtual ICollection<ProfessionalPerson> ProfessionalPersons { get; set; }

        /// <summary>
        /// Diagnosticos realizados por este profesional.
        /// </summary>
        public virtual ICollection<Diagnosis> Diagnoses { get; set; }

        /// <summary>
        /// Actividades creadas por este profesional.
        /// </summary>
        public virtual ICollection<Activity> Activities { get; set; }

        /// <summary>
        /// Asignaciones de actividades realizadas por este profesional.
        /// </summary>
        public virtual ICollection<ActivityAssignment> ActivityAssignments { get; set; }

        /// <summary>
        /// Reportes generados por este profesional.
        /// </summary>
        public virtual ICollection<Report> Reports { get; set; }

        /// <summary>
        /// Invitaciones creadas por este profesional para registrar familiares.
        /// </summary>
        public virtual ICollection<Invitation> CreatedInvitations { get; set; }

        public Professional()
        {
            Id = Guid.NewGuid();
            ProfessionalInstitutions = new HashSet<ProfessionalInstitution>();
            ProfessionalPersons = new HashSet<ProfessionalPerson>();
            Diagnoses = new HashSet<Diagnosis>();
            Activities = new HashSet<Activity>();
            ActivityAssignments = new HashSet<ActivityAssignment>();
            Reports = new HashSet<Report>();
            CreatedInvitations = new HashSet<Invitation>();
        }
    }
}
