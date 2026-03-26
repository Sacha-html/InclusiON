using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para asignar una persona a un profesional.
    /// </summary>
    public class AssignPersonRequest
    {
        [Required(ErrorMessage = "El ID de la persona es requerido")]
        public Guid PersonId { get; set; }

        public bool IsPrimaryProfessional { get; set; } = false;

        public bool CanSuperviseLogin { get; set; } = false;
    }
}
