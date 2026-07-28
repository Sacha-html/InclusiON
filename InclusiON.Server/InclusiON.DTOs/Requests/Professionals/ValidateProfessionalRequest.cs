using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Professionals
{
    public class ValidateProfessionalRequest
    {
        [Required(ErrorMessage = "Debe especificar si se aprueba o rechaza")]
        public bool IsApproved { get; set; }

        [MaxLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres")]
        public string? Observation { get; set; }
    }
}