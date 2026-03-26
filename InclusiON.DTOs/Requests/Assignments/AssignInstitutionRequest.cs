using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para asignar una institucion a un profesional.
    /// </summary>
    public class AssignInstitutionRequest
    {
        [Required(ErrorMessage = "El ID de la institucion es requerido")]
        public int InstitutionId { get; set; }
    }
}
