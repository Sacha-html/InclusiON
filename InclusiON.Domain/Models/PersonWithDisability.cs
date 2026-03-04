using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Persona con discapacidad. Entidad central del sistema que representa al beneficiario.
    /// Contiene el perfil funcional, preferencias de accesibilidad y configuracion de acceso adaptativo.
    /// </summary>
    public class PersonWithDisability : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la persona.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del usuario asociado a este perfil.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre de la persona.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido de la persona.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Numero de documento de identidad.
        /// </summary>
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Fecha de nacimiento.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// ID del tipo de discapacidad (catalogo).
        /// </summary>
        public int? DisabilityTypeId { get; set; }

        /// <summary>
        /// URL de la foto de perfil.
        /// </summary>
        public string? PhotoUrl { get; set; }

        #region Perfil Funcional
        /// <summary>
        /// Nivel de atencion (1-5). Capacidad de mantener la concentracion.
        /// </summary>
        public int? AttentionLevel { get; set; }

        /// <summary>
        /// Nivel de comunicacion (1-5). Capacidad de expresarse y comprender.
        /// </summary>
        public int? CommunicationLevel { get; set; }

        /// <summary>
        /// Indica si utiliza Comunicacion Aumentativa y Alternativa (CAA).
        /// </summary>
        public bool UsesAAC { get; set; }

        /// <summary>
        /// Indica si utiliza lengua de senas.
        /// </summary>
        public bool UsesSignLanguage { get; set; }

        /// <summary>
        /// Nivel de motricidad (1-5). Capacidad de movimiento y coordinacion.
        /// </summary>
        public int? MotorSkillLevel { get; set; }
        #endregion

        #region Preferencias y Motivadores
        /// <summary>
        /// Intereses y motivadores de la persona (texto libre).
        /// </summary>
        public string? InterestsAndMotivators { get; set; }

        /// <summary>
        /// Estilo de aprendizaje preferido (Visual, Auditivo, Kinestesico).
        /// </summary>
        public string? LearningStyle { get; set; }

        /// <summary>
        /// Recursos disponibles para el aprendizaje.
        /// </summary>
        public string? AvailableResources { get; set; }

        /// <summary>
        /// Terapias adicionales que recibe la persona.
        /// </summary>
        public string? AdditionalTherapies { get; set; }
        #endregion

        #region Ajustes Razonables (Accesibilidad)
        /// <summary>
        /// Requiere fuente de texto grande.
        /// </summary>
        public bool RequiresLargeFont { get; set; }

        /// <summary>
        /// Requiere alto contraste en la interfaz.
        /// </summary>
        public bool RequiresHighContrast { get; set; }

        /// <summary>
        /// Sensibilidad al ruido visual (interfaces recargadas).
        /// </summary>
        public bool VisualNoiseSensitivity { get; set; }

        /// <summary>
        /// Sensibilidad al sonido.
        /// </summary>
        public bool SoundSensitivity { get; set; }
        #endregion

        #region Configuracion de Acceso
        /// <summary>
        /// ID del nivel de autonomia (catalogo). Determina el tipo de login.
        /// </summary>
        public int? AutonomyLevelId { get; set; }

        /// <summary>
        /// ID del metodo de login configurado (catalogo).
        /// </summary>
        public int? LoginMethodId { get; set; }

        /// <summary>
        /// Hash del PIN numerico (4-6 digitos) para login simplificado.
        /// </summary>
        public string? PinCodeHash { get; set; }

        /// <summary>
        /// Secuencia de emojis para login visual. Formato JSON: ["🐶","🏠","🌻","🍎"].
        /// </summary>
        public string? EmojiSequence { get; set; }

        /// <summary>
        /// ID de la combinacion color-forma para login visual (1-24).
        /// </summary>
        public int? ColorShapeId { get; set; }

        /// <summary>
        /// Color del avatar del usuario en formato hexadecimal.
        /// </summary>
        public string? AvatarColor { get; set; }

        /// <summary>
        /// ID del usuario supervisor que puede desbloquear el login supervisado.
        /// </summary>
        public Guid? SupervisorUserId { get; set; }
        #endregion

        #region Navegacion
        /// <summary>
        /// Usuario asociado a este perfil.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Usuario supervisor para login asistido.
        /// </summary>
        public virtual User? SupervisorUser { get; set; }

        /// <summary>
        /// Tipo de discapacidad (catalogo).
        /// </summary>
        public virtual DisabilityType? DisabilityType { get; set; }

        /// <summary>
        /// Nivel de autonomia (catalogo).
        /// </summary>
        public virtual AutonomyLevel? AutonomyLevel { get; set; }

        /// <summary>
        /// Metodo de login configurado (catalogo).
        /// </summary>
        public virtual LoginMethod? LoginMethod { get; set; }

        /// <summary>
        /// Representantes familiares asociados.
        /// </summary>
        public virtual ICollection<PersonRepresentative> PersonRepresentatives { get; set; }

        /// <summary>
        /// Profesionales asignados a esta persona.
        /// </summary>
        public virtual ICollection<ProfessionalPerson> ProfessionalPersons { get; set; }

        /// <summary>
        /// Diagnosticos realizados a esta persona.
        /// </summary>
        public virtual ICollection<Diagnosis> Diagnoses { get; set; }

        /// <summary>
        /// Actividades asignadas a esta persona.
        /// </summary>
        public virtual ICollection<ActivityAssignment> ActivityAssignments { get; set; }

        /// <summary>
        /// Reportes generados sobre esta persona.
        /// </summary>
        public virtual ICollection<Report> Reports { get; set; }

        /// <summary>
        /// Mensajes relacionados con esta persona.
        /// </summary>
        public virtual ICollection<Message> RelatedMessages { get; set; }

        /// <summary>
        /// Registros de auditoria de acceso a datos de esta persona.
        /// </summary>
        public virtual ICollection<AccessAudit> AccessAudits { get; set; }
        #endregion

        public PersonWithDisability()
        {
            Id = Guid.NewGuid();
            UsesAAC = false;
            UsesSignLanguage = false;
            RequiresLargeFont = false;
            RequiresHighContrast = false;
            VisualNoiseSensitivity = false;
            SoundSensitivity = false;
            PersonRepresentatives = new HashSet<PersonRepresentative>();
            ProfessionalPersons = new HashSet<ProfessionalPerson>();
            Diagnoses = new HashSet<Diagnosis>();
            ActivityAssignments = new HashSet<ActivityAssignment>();
            Reports = new HashSet<Report>();
            RelatedMessages = new HashSet<Message>();
            AccessAudits = new HashSet<AccessAudit>();
        }
    }
}
