namespace InclusiON.DTOs.Responses.Persons
{
    /// <summary>
    /// Response con los datos de una persona con discapacidad.
    /// </summary>
    public class PersonResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? DocumentNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age => CalculateAge(BirthDate);
        public string? PhotoUrl { get; set; }

        #region Perfil Funcional
        public int? AttentionLevel { get; set; }
        public int? CommunicationLevel { get; set; }
        public bool UsesAAC { get; set; }
        public bool UsesSignLanguage { get; set; }
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
        public string? AutonomyLevelName { get; set; }
        public int? LoginMethodId { get; set; }
        public string? LoginMethodName { get; set; }
        public bool HasPinConfigured { get; set; }
        public Guid? SupervisorUserId { get; set; }
        public string? SupervisorName { get; set; }
        public string? AvatarColor { get; set; }
        #endregion

        #region Tipo de Discapacidad
        public int? DisabilityTypeId { get; set; }
        public string? DisabilityTypeName { get; set; }
        #endregion

        #region Estado
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        #endregion

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
