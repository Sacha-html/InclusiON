using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Family
{
    public class UnlinkFamilyFromPersonRequest
    {
        [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres")]
        public string Observation { get; set; } = string.Empty;
    }
}
