namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Catalogo de niveles de autonomia.
    /// Determina el tipo de login y supervision requerida para una persona con discapacidad.
    /// Valores: Alta (independiente), Media (login simplificado), Baja (requiere supervision).
    /// </summary>
    public class AutonomyLevel
    {
        /// <summary>
        /// Identificador unico del nivel de autonomia.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del nivel (ej: Alta, Media, Baja).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion detallada de las caracteristicas de este nivel.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si este nivel requiere supervision de un familiar o profesional.
        /// </summary>
        public bool RequiresSupervision { get; set; }

        /// <summary>
        /// Orden de visualizacion en listas.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indica si el nivel esta activo para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Personas con discapacidad que tienen este nivel de autonomia.
        /// </summary>
        public virtual ICollection<PersonWithDisability> PersonsWithDisability { get; set; }

        public AutonomyLevel()
        {
            PersonsWithDisability = new HashSet<PersonWithDisability>();
        }
    }
}
