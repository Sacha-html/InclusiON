namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonAccessibilityCommand(
        Guid    PersonId,
        bool    RequiresLargeFont,
        bool    RequiresHighContrast,
        bool    VisualNoiseSensitivity,
        bool    SoundSensitivity,
        string? ColorBlindnessType);
}
