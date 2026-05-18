using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Persons
{
    /// <summary>
    /// Response resumido para listados de personas.
    /// </summary>
    public class PersonListItemResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{LastName}, {FirstName}".Trim();
        public string? DocumentNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age => CalculateAge(BirthDate);
        public string? PhotoUrl { get; set; }
        public string? AvatarColor { get; set; }

        public int? DisabilityTypeId { get; set; }
        public string? DisabilityTypeName { get; set; }

        public int? AutonomyLevelId { get; set; }
        public string? AutonomyLevelName { get; set; }

        public string? LoginMethodName { get; set; }

        public bool IsActive { get; set; }

        public string? RepresentativeNames { get; set; }

        public string? EncryptedId { get; set; }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public static PersonListItemResponse MapToResponse(PersonWithDisability p)
        {
            return new PersonListItemResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DocumentNumber = p.DocumentNumber,
                BirthDate = p.BirthDate,
                PhotoUrl = p.PhotoUrl,
                AvatarColor = p.AvatarColor,
                DisabilityTypeId = p.DisabilityTypeId,
                DisabilityTypeName = p.DisabilityType?.Name,
                AutonomyLevelId = p.AutonomyLevelId,
                AutonomyLevelName = p.AutonomyLevel?.Name,
                LoginMethodName = p.LoginMethod?.Name,
                IsActive = p.User?.IsActive ?? false,
                RepresentativeNames = p.PersonRepresentatives != null && p.PersonRepresentatives.Any(pr => pr.IsActive)
                    ? string.Join(", ", p.PersonRepresentatives
                        .Where(pr => pr.IsActive)
                        .Select(pr => $"{pr.Representative.LastName}, {pr.Representative.FirstName}".Trim()))
                    : null
            };
        }
    }
}
