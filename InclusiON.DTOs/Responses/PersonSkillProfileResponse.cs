namespace InclusiON.DTOs.Responses
{
    public class PersonSkillProfileResponse
    {
        public int SkillAreaId { get; set; }
        public string SkillAreaName { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
