using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catalogo de categorias de actividades.
    /// Agrupa las actividades por area de desarrollo o habilidad.
    /// </summary>
    public class ActivityCategory : NameableEntity
    {
        /// <summary>
        /// Descripcion de la categoria y su objetivo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si la categoria esta activa para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Actividades que pertenecen a esta categoria.
        /// </summary>
        public virtual ICollection<Activity> Activities { get; set; }

        public ActivityCategory()
        {
            Activities = new HashSet<Activity>();
        }
    }
}
