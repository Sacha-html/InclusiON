using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    public class UpdatePersonAccessConfigurationRequest
    {
        [Range(1, int.MaxValue)]
        public int AutonomyLevelId { get; set; }
        [Range(1, 3)]
        public int LoginMethodId { get; set; }
        public string? Pin { get; set; }
        public Guid? SupervisorUserId { get; set; }
        [Required]
        public string AvatarColor { get; set; } = string.Empty;
    }
}
