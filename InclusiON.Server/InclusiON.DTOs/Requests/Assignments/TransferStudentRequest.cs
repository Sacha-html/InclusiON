using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para transferir un alumno de un profesional a otro.
    /// </summary>
    public class TransferStudentRequest
    {
        [Required(ErrorMessage = "El ID de la persona es requerido")]
        public Guid PersonId { get; set; }

        [Required(ErrorMessage = "El ID del profesional origen es requerido")]
        public Guid FromProfessionalId { get; set; }

        [Required(ErrorMessage = "El ID del profesional destino es requerido")]
        public Guid ToProfessionalId { get; set; }
    }
}
