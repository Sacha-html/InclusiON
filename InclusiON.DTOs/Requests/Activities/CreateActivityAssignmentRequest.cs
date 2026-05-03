using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Activities
{
    public class CreateActivityAssignmentRequest
    {
        [Required(ErrorMessage = "La actividad es requerida")]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "La persona es requerida")]
        public Guid PersonId { get; set; }

        public DateTime? DueDate { get; set; }
        public bool IsEvaluationActivity { get; set; } = false;
        public int? SequenceOrder { get; set; }
    }
}
