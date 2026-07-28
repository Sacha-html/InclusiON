namespace InclusiON.DTOs.Responses.Persons
{
    public class PersonProfessionalResponse
    {
        public Guid ProfessionalId { get; set; }
        public Guid PersonId { get; set; }
        public string PersonFirstName { get; set; } = string.Empty;
        public string PersonLastName { get; set; } = string.Empty;
        public string PersonFullName { get; set; } = string.Empty;
        public bool IsPrimaryProfessional { get; set; }
        public bool CanSuperviseLogin { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}