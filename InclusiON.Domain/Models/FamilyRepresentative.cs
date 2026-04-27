using InclusiON.Domain.Enums;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Representante familiar de una o mas personas con discapacidad.
    /// Puede ser padre, madre, tutor legal u otro familiar responsable.
    /// Tiene acceso a ver el progreso y puede supervisar el login de la persona.
    /// </summary>
    public class FamilyRepresentative : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del representante familiar.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del usuario asociado a este perfil.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre del representante.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del representante.
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
        /// Parentesco con la persona representada (ej: Madre, Padre, Tutor, Abuelo).
        /// </summary>
        public string? Relationship { get; set; }

        /// <summary>
        /// Estado del familiar en el sistema.
        /// </summary>
        public FamilyStatusEnum Status { get; set; } = FamilyStatusEnum.Active;

        /// <summary>
        /// Historial de cambios de estado del familiar.
        /// </summary>
        public virtual ICollection<FamilyStatusHistory> StatusHistory { get; set; }

        /// <summary>
        /// Usuario asociado a este perfil.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Personas con discapacidad que representa.
        /// </summary>
        public virtual ICollection<PersonRepresentative> PersonRepresentatives { get; set; }

        public FamilyRepresentative()
        {
            Id = Guid.NewGuid();
            PersonRepresentatives = new HashSet<PersonRepresentative>();
            StatusHistory = new HashSet<FamilyStatusHistory>();
        }
    }
}
