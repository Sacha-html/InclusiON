using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Professionals
{
    public class GetPendingProfessionalsRequest : PagedRequest
    {
        public string? Search { get; set; }
    }
}