using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Family
{
    public class GetFamilyRequest : PagedRequest
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }
}
