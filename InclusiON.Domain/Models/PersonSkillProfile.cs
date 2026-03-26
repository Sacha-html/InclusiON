namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Perfil de habilidades de una persona: areas de habilidad asignadas.
    /// </summary>
    public class PersonSkillProfile
    {
        public Guid PersonId { get; set; }
        public int SkillAreaId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual PersonWithDisability Person { get; set; } = null!;
        public virtual SkillArea SkillArea { get; set; } = null!;
    }
}
