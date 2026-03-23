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
        public string? AvatarColor { get; set; }
        public string? DisabilityTypeName { get; set; }
        public int? Age { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsPrimaryProfessional { get; set; }
        public bool CanSuperviseLogin { get; set; }
        public bool IsActive { get; set; }
    }
}
