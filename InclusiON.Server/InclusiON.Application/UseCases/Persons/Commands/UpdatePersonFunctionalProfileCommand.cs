namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonFunctionalProfileCommand(
        Guid PersonId,
        int? AttentionLevel,
        int? CommunicationLevel,
        bool UsesAAC,
        bool UsesSignLanguage,
        int? MotorSkillLevel,
        string? InterestsAndMotivators,
        string? LearningStyle,
        string? AvailableResources,
        string? AdditionalTherapies,
        bool RequiresLargeFont,
        bool RequiresHighContrast,
        bool VisualNoiseSensitivity,
        bool SoundSensitivity,
        string? ColorBlindnessType);
}
