using InclusiON.Domain.Models;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.DTOs.Responses.Catalogs
{
    public class CatalogItemResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public static CatalogItemResponse MapToResponse(ActivityCategory x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        public static CatalogItemResponse MapToResponse(DisabilityType x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        public static CatalogItemResponse MapToResponse(LoginMethod x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        public static CatalogItemResponse MapToResponse(ReportType x)
        {
            return new CatalogItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }
    }
}
