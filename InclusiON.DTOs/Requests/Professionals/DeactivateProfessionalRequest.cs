using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Professionals
{
    public class DeactivateProfessionalRequest
    {
        [MaxLength(500)]
        public string? Observation { get; set; }
    }
}