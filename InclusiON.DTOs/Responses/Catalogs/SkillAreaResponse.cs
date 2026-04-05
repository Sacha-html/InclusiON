using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Catalogs
{
    public class SkillAreaResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }

        public static SkillAreaResponse MapToResponse(SkillArea x)
        {
            return new SkillAreaResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Icon = x.Icon,
                Color = x.Color,
                DisplayOrder = x.DisplayOrder
            };
        }
    }
}
