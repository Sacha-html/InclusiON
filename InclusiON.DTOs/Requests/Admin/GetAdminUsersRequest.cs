using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Admin
{
    public class GetAdminUsersRequest : PagedRequest, IInstitutionFilterable
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int? InstitutionId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? InstitutionIds { get; set; }
    }
}
