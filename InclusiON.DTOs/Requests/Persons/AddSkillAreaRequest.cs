using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    public class AddSkillAreaRequest
    {
        [Required]
        public int SkillAreaId { get; set; }
    }
}
