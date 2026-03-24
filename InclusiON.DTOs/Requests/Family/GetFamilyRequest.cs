using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Family
{
    public class GetFamilyRequest : PagedRequest, IInstitutionFilterable
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public int? InstitutionId { get; set; }

        /// <summary>
        /// IDs de instituciones validados por el filter (no se bindea desde query string).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? InstitutionIds { get; set; }
    }
}
