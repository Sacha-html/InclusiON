using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para crear una persona con discapacidad.
    /// </summary>
    public class CreatePersonRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20, MinimumLength = 6, ErrorMessage = "El documento debe tener entre 6 y 20 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El documento solo puede contener letras y números")]
        public string? DocumentNumber { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime BirthDate { get; set; }

        public int? DisabilityTypeId { get; set; }

        public string? PhotoUrl { get; set; }

        #region Perfil Funcional
        [Range(1, 5, ErrorMessage = "El nivel de atencion debe estar entre 1 y 5")]
        public int? AttentionLevel { get; set; }

        [Range(1, 5, ErrorMessage = "El nivel de comunicacion debe estar entre 1 y 5")]
        public int? CommunicationLevel { get; set; }

        public bool UsesAAC { get; set; }

        public bool UsesSignLanguage { get; set; }

        [Range(1, 5, ErrorMessage = "El nivel de motricidad debe estar entre 1 y 5")]
        public int? MotorSkillLevel { get; set; }
        #endregion

        #region Preferencias
        public string? InterestsAndMotivators { get; set; }
        public string? LearningStyle { get; set; }
        public string? AvailableResources { get; set; }
        public string? AdditionalTherapies { get; set; }
        #endregion

        #region Accesibilidad
        public bool RequiresLargeFont { get; set; }
        public bool RequiresHighContrast { get; set; }
        public bool VisualNoiseSensitivity { get; set; }
        public bool SoundSensitivity { get; set; }
        public string? ColorBlindnessType { get; set; }
        #endregion

        #region Configuracion de Acceso
        public int? AutonomyLevelId { get; set; }
        public int? LoginMethodId { get; set; }

        [StringLength(6, MinimumLength = 4, ErrorMessage = "El PIN debe tener entre 4 y 6 digitos")]
        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "El PIN debe contener solo digitos")]
        public string? Pin { get; set; }

        public Guid? SupervisorUserId { get; set; }
        public string? AvatarColor { get; set; }
        #endregion
    }
}
