using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catálogo de estados de asignación de actividad.
    /// </summary>
    public class ActivityAssignmentStatus : NameableEntity
    {
        public virtual ICollection<ActivityAssignment> Assignments { get; set; }

        public ActivityAssignmentStatus()
        {
            Assignments = new HashSet<ActivityAssignment>();
        }
    }
}
