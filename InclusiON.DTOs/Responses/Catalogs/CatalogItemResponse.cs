using InclusiON.Domain.Models;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.DTOs.Responses.Catalogs
{
    public class CatalogItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public static CatalogItemResponse MapToResponse(ActivityCategory x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            };
        }

        public static CatalogItemResponse MapToResponse(DisabilityType x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            };
        }

        public static CatalogItemResponse MapToResponse(LoginMethod x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            };
        }
    }
}
