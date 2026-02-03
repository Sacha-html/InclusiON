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
        public string FullName => $"{FirstName} {LastName}".Trim();
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

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
