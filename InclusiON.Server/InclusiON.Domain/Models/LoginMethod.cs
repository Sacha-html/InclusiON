using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catalogo de metodos de autenticacion disponibles.
    /// Cada metodo esta adaptado a diferentes niveles de autonomia.
    /// Valores: STANDARD, PIN, ASSISTED.
    /// </summary>
    public class LoginMethod : IActivatable
    {
        public int Id { get; set; }

        /// <summary>
        /// Codigo unico del metodo (ej: STANDARD, PIN, ASSISTED).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del metodo para mostrar al usuario.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion detallada del funcionamiento del metodo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Nivel minimo de autonomia requerido para usar este metodo.
        /// </summary>
        public int MinAutonomyLevel { get; set; }

        /// <summary>
        /// Indica si requiere email para autenticarse.
        /// </summary>
        public bool RequiresEmail { get; set; }

        /// <summary>
        /// Indica si requiere contraseña tradicional.
        /// </summary>
        public bool RequiresPassword { get; set; }

        /// <summary>
        /// Indica si requiere PIN numerico.
        /// </summary>
        public bool RequiresPin { get; set; }

        /// <summary>
        /// Indica si requiere supervision de un familiar o profesional.
        /// </summary>
        public bool RequiresSupervisor { get; set; }

        /// <summary>
        /// Orden de visualizacion en listas.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indica si el metodo esta activo para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Personas con discapacidad que usan este metodo de login.
        /// </summary>
        public virtual ICollection<PersonWithDisability> PersonsWithDisability { get; set; }

        public LoginMethod()
        {
            PersonsWithDisability = new HashSet<PersonWithDisability>();
        }
    }
}
