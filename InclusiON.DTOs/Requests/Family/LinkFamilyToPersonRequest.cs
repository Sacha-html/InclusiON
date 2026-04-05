using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Family
{
    public class LinkFamilyToPersonRequest
    {
        [Required(ErrorMessage = "La relación es requerida")]
        [StringLength(50, ErrorMessage = "La relación no puede exceder 50 caracteres")]
        public string Relationship { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
    }
}
