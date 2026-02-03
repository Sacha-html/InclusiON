namespace InclusiON.ApplicationBusiness.UseCases.Persons.Commands
{
    public record CreatePersonCommand(
        string FirstName,
        string LastName,
        string? DocumentNumber,
        DateTime BirthDate,
        int? DisabilityTypeId,
        string? PhotoUrl,
        // Perfil funcional
        int? AttentionLevel,
        int? CommunicationLevel,
        bool UsesAAC,
        bool UsesSignLanguage,
        int? MotorSkillLevel,
        // Preferencias
        string? InterestsAndMotivators,
        string? LearningStyle,
        string? AvailableResources,
        string? AdditionalTherapies,
        // Accesibilidad
        bool RequiresLargeFont,
        bool RequiresHighContrast,
        bool VisualNoiseSensitivity,
        bool SoundSensitivity,
        // Configuracion de acceso
        int? AutonomyLevelId,
        int? LoginMethodId,
        string? Pin,
        Guid? SupervisorUserId,
        string? AvatarColor
    );
}
