using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

/// <summary>Specialty available for professional profiles.</summary>
public class Specialty : NameableEntity, IActivatable
{
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Professional> Professionals { get; set; } = new HashSet<Professional>();
}
