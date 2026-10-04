using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

/// <summary>
/// Rol o cargo institucional de un miembro del equipo directivo o administrativo escolar.
/// </summary>
public class InstitutionalRole : NameableEntity, IActivatable
{
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public virtual ICollection<AdminInstitution> AdminInstitutions { get; set; } = new HashSet<AdminInstitution>();
}
