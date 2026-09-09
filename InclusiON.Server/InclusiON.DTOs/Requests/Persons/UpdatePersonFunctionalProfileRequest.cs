using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Datos funcionales evaluados por el profesional principal asignado al alumno.
    /// No incluye credenciales, permisos ni configuracion de inicio de sesion.
    /// </summary>
    public class UpdatePersonFunctionalProfileRequest
    {
        [Range(1, 5, ErrorMessage = "El nivel de atencion debe estar entre 1 y 5")]
        public int? AttentionLevel { get; set; }

        [Range(1, 5, ErrorMessage = "El nivel de comunicacion debe estar entre 1 y 5")]
        public int? CommunicationLevel { get; set; }

        public bool UsesAAC { get; set; }
        public bool UsesSignLanguage { get; set; }

        [Range(1, 5, ErrorMessage = "El nivel de motricidad debe estar entre 1 y 5")]
        public int? MotorSkillLevel { get; set; }

        public string? InterestsAndMotivators { get; set; }
        public string? LearningStyle { get; set; }
        public string? AvailableResources { get; set; }
        public string? AdditionalTherapies { get; set; }

        public bool RequiresLargeFont { get; set; }
        public bool RequiresHighContrast { get; set; }
        public bool VisualNoiseSensitivity { get; set; }
        public bool SoundSensitivity { get; set; }
        public string? ColorBlindnessType { get; set; }
    }
}
