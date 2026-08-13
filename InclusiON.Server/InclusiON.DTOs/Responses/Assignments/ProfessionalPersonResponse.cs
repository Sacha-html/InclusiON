using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Assignments
{
    /// <summary>
    /// Response con los datos de una asignacion profesional-persona.
    /// </summary>
    public class ProfessionalPersonResponse
    {
        public Guid ProfessionalId { get; set; }
        public Guid PersonId { get; set; }
        public string PersonFirstName { get; set; } = string.Empty;
        public string PersonLastName { get; set; } = string.Empty;
        public string PersonFullName => $"{PersonFirstName} {PersonLastName}".Trim();
        public string? PersonDocumentNumber { get; set; }
        public string? AvatarColor { get; set; }
        public string? DisabilityTypeName { get; set; }
        public int? Age { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsPrimaryProfessional { get; set; }
        public bool CanSuperviseLogin { get; set; }
        public bool IsActive { get; set; }
        public Guid? ClassroomId { get; set; }
        public string? ClassroomName { get; set; }

        public static ProfessionalPersonResponse MapToResponse(ProfessionalPerson assignment)
        {
            var person = assignment.Person;
            int? age = null;
            if (person != null && person.BirthDate != default)
            {
                var today = DateTime.UtcNow;
                age = today.Year - person.BirthDate.Year;
                if (person.BirthDate.Date > today.AddYears(-age.Value)) age--;
            }

            return new ProfessionalPersonResponse
            {
                ProfessionalId = assignment.ProfessionalId,
                PersonId = assignment.PersonId,
                PersonFirstName = person?.FirstName ?? string.Empty,
                PersonLastName = person?.LastName ?? string.Empty,
                PersonDocumentNumber = person?.DocumentNumber,
                AvatarColor = person?.AvatarColor,
                DisabilityTypeName = person?.DisabilityType?.Name,
                Age = age,
                AssignedAt = assignment.AssignedAt,
                IsPrimaryProfessional = assignment.IsPrimaryProfessional,
                CanSuperviseLogin = assignment.CanSuperviseLogin,
                IsActive = assignment.IsActive,
                ClassroomId = assignment.ClassroomId,
                ClassroomName = assignment.Classroom?.Name
            };
        }
    }
}
