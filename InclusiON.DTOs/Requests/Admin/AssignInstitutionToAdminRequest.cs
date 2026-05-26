using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Admin
{
    public class AssignInstitutionToAdminRequest
    {
        [Required(ErrorMessage = "El ID de la institucion es requerido")]
        public string InstitutionId { get; set; } = string.Empty;
    }
}
