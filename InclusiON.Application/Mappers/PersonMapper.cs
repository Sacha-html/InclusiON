using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.Mappers
{
    /// <summary>
    /// Mapper centralizado para convertir PersonWithDisability a PersonResponse.
    /// Maneja correctamente navigation properties que pueden no estar cargadas.
    /// </summary>
    public static class PersonMapper
    {
        public static PersonResponse ToResponse(PersonWithDisability person)
        {
            return new PersonResponse
            {
                Id = person.Id,
                UserId = person.UserId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                DocumentNumber = person.DocumentNumber,
                BirthDate = person.BirthDate,
                PhotoUrl = person.PhotoUrl,
                // Perfil funcional
                AttentionLevel = person.AttentionLevel,
                CommunicationLevel = person.CommunicationLevel,
                UsesAAC = person.UsesAAC,
                UsesSignLanguage = person.UsesSignLanguage,
                MotorSkillLevel = person.MotorSkillLevel,
                // Preferencias
                InterestsAndMotivators = person.InterestsAndMotivators,
                LearningStyle = person.LearningStyle,
                AvailableResources = person.AvailableResources,
                AdditionalTherapies = person.AdditionalTherapies,
                // Accesibilidad
                RequiresLargeFont = person.RequiresLargeFont,
                RequiresHighContrast = person.RequiresHighContrast,
                VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                SoundSensitivity = person.SoundSensitivity,
                ColorBlindnessType = person.ColorBlindnessType,
                // Configuracion de acceso
                AutonomyLevelId = person.AutonomyLevelId,
                AutonomyLevelName = person.AutonomyLevel?.Name,
                LoginMethodId = person.LoginMethodId,
                LoginMethodName = person.LoginMethod?.Name,
                HasPinConfigured = !string.IsNullOrEmpty(person.PinCodeHash),
                SupervisorUserId = person.SupervisorUserId,
                SupervisorName = person.SupervisorUser != null
                    ? $"{person.SupervisorUser.Name} {person.SupervisorUser.Surname}".Trim()
                    : null,
                AvatarColor = person.AvatarColor,
                // Tipo de discapacidad
                DisabilityTypeId = person.DisabilityTypeId,
                DisabilityTypeName = person.DisabilityType?.Name,
                // Estado
                IsActive = person.User?.IsActive ?? true,
                CreatedAt = person.CreatedAt,
                UpdatedAt = person.UpdatedAt
            };
        }
    }
}
