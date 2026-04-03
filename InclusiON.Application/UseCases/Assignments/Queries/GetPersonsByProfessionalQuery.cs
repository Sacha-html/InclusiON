using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Queries
{
    public record GetPersonsByProfessionalQuery(Guid ProfessionalId)
    {
        internal static ProfessionalPersonResponse MapToResponse(ProfessionalPerson assignment)
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
                AvatarColor = person?.AvatarColor,
                DisabilityTypeName = person?.DisabilityType?.Name,
                Age = age,
                AssignedAt = assignment.AssignedAt,
                IsPrimaryProfessional = assignment.IsPrimaryProfessional,
                CanSuperviseLogin = assignment.CanSuperviseLogin,
                IsActive = assignment.IsActive
            };
        }
    }
}
