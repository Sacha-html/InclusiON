using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Roadmap
{
    /// <summary>
    /// Cuerpo del request para asignar directamente una actividad del roadmap al alumno.
    /// </summary>
    public class AssignFromRoadmapRequest
    {
        /// <summary>Fecha límite opcional para completar la actividad.</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Indica si la actividad cuenta como evaluación.</summary>
        public bool IsEvaluationActivity { get; set; }

        public bool BypassDuplicateWarning { get; set; } = false;
    }
}
