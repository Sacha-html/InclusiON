namespace InclusiON.Entities.Models
{
    /// <summary>
    /// Catalogo de metodos de autenticacion disponibles.
    /// Cada metodo esta adaptado a diferentes niveles de autonomia.
    /// Valores: STANDARD, PIN, EMOJI_SEQUENCE, COLOR_SHAPE, SUPERVISED, TRUSTED_DEVICE, PROFILE_SELECT.
    /// </summary>
    public class LoginMethod
    {
        /// <summary>
        /// Identificador unico del metodo de login.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Codigo unico del metodo (ej: STANDARD, PIN, EMOJI_SEQUENCE, COLOR_SHAPE).
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
        /// Indica si requiere contrasena tradicional.
        /// </summary>
        public bool RequiresPassword { get; set; }

        /// <summary>
        /// Indica si requiere PIN numerico.
        /// </summary>
        public bool RequiresPin { get; set; }

        /// <summary>
        /// Indica si requiere secuencia de emojis.
        /// </summary>
        public bool RequiresEmojiSequence { get; set; }

        /// <summary>
        /// Indica si requiere secuencia de colores y formas.
        /// </summary>
        public bool RequiresColorShape { get; set; }

        /// <summary>
        /// Indica si requiere seleccion de perfil (nombre/avatar).
        /// </summary>
        public bool RequiresProfileSelect { get; set; }

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
