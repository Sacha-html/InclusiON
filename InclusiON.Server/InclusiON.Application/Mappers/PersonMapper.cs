using InclusiON.Application.Constants;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.Mappers
{
    /// <summary>
    /// Mapper centralizado para el dominio de personas.
    /// </summary>
    public static class PersonMapper
    {
        public static PersonProfessionalResponse ToProfessionalResponse(ProfessionalPerson pp) => new()
        {
            ProfessionalId        = pp.ProfessionalId,
            PersonId              = pp.PersonId,
            PersonFirstName       = pp.Professional.FirstName,
            PersonLastName        = pp.Professional.LastName,
            PersonFullName        = $"{pp.Professional.FirstName} {pp.Professional.LastName}",
            IsPrimaryProfessional = pp.IsPrimaryProfessional,
            CanSuperviseLogin     = pp.CanSuperviseLogin,
            IsActive              = pp.IsActive,
            AssignedAt            = pp.AssignedAt,
        };

        public static PersonSkillProfileResponse ToSkillProfileResponse(PersonSkillProfile psp) => new()
        {
            SkillAreaId   = psp.SkillAreaId,
            SkillAreaName = psp.SkillArea.Name,
            Color         = psp.SkillArea.Color,
            Icon          = psp.SkillArea.Icon,
            IsActive      = psp.IsActive,
            AssignedAt    = psp.AssignedAt,
        };

        /// <summary>
        /// Overload para cuando SkillArea no está cargada como navigation property.
        /// </summary>
        public static PersonSkillProfileResponse ToSkillProfileResponse(PersonSkillProfile psp, SkillArea area) => new()
        {
            SkillAreaId   = psp.SkillAreaId,
            SkillAreaName = area.Name,
            Color         = area.Color,
            Icon          = area.Icon,
            IsActive      = psp.IsActive,
            AssignedAt    = psp.AssignedAt,
        };

        public static SupervisorCandidateResponse ToSupervisorCandidate(Professional p) => new()
        {
            UserId   = p.UserId,
            FullName = $"{p.FirstName} {p.LastName}",
            Type     = RoleNames.Professional,
        };

        public static SupervisorCandidateResponse ToSupervisorCandidate(PersonRepresentative pr) => new()
        {
            UserId       = pr.Representative.UserId,
            FullName     = $"{pr.Representative.FirstName} {pr.Representative.LastName}",
            Type         = RoleNames.Family,
            Relationship = pr.Relationship,
        };

        public static PersonRepresentativeResponse ToRepresentativeResponse(PersonRepresentative pr) => new()
        {
            PersonId               = pr.PersonId,
            RepresentativeId       = pr.RepresentativeId,
            RepresentativeFullName = $"{pr.Representative.FirstName} {pr.Representative.LastName}",
            RepresentativeDocumentNumber = pr.Representative.DocumentNumber,
            RepresentativeEmail = pr.Representative.User?.Email,
            Relationship           = pr.Relationship,
            IsPrimary              = pr.IsPrimary,
            IsActive               = pr.IsActive,
            CreatedAt              = pr.CreatedAt,
            UpdatedAt              = pr.UpdatedAt,
            EndedAt                = pr.EndedAt,
            UnlinkObservation      = pr.UnlinkObservation,
        };

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
