using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Activities
{
    public class GetActivitiesRequest : PagedRequest
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SkillAreaId { get; set; }
        public int? TemplateTypeId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsStandard { get; set; }
        public bool? IsTemplate { get; set; }
    }
}
