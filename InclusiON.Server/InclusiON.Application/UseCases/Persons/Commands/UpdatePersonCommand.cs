namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonCommand(
        Guid PersonId,
        string? FirstName,
        string? LastName,
        string? DocumentNumber,
        DateTime? BirthDate,
        int? DisabilityTypeId,
        string? PhotoUrl,
        // Perfil funcional
        int? AttentionLevel,
        int? CommunicationLevel,
        bool? UsesAAC,
        bool? UsesSignLanguage,
        int? MotorSkillLevel,
        // Preferencias
        string? InterestsAndMotivators,
        string? LearningStyle,
        string? AvailableResources,
        string? AdditionalTherapies,
        // Accesibilidad
        bool? RequiresLargeFont,
        bool? RequiresHighContrast,
        bool? VisualNoiseSensitivity,
        bool? SoundSensitivity,
        string? ColorBlindnessType,
        // Configuracion de acceso
        int? AutonomyLevelId,
        Guid? SupervisorUserId,
        string? AvatarColor
    );
}
