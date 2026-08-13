using System;

namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record CreatePersonWithTutorCommand(
        // Alumno
        string FirstName,
        string LastName,
        string? DocumentNumber,
        DateTime BirthDate,
        int? DisabilityTypeId,
        string? PhotoUrl,
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
        string? ColorBlindnessType,
        int? AutonomyLevelId,
        int? LoginMethodId,
        string? Pin,
        string? AvatarColor,

        // Tutor
        string TutorFirstName,
        string TutorLastName,
        string TutorEmail,
        string? TutorDocumentNumber,
        string? TutorPhone,
        string TutorRelationship,

        // Asignación de Aula
        Guid? ClassroomId
    );
}
