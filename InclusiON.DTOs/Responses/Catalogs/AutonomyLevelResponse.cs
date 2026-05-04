using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Catalogs
{
    public class AutonomyLevelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresSupervision { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public static AutonomyLevelResponse MapToResponse(AutonomyLevel x)
        {
            return new AutonomyLevelResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                RequiresSupervision = x.RequiresSupervision,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            };
        }
    }
}
