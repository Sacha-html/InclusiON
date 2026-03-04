namespace InclusiON.DTOs.Responses.Auth
{
    /// <summary>
    /// Respuesta con informacion de un metodo de login.
    /// </summary>
    public class LoginMethodResponse
    {
        /// <summary>
        /// ID del metodo de login.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Codigo unico del metodo (STANDARD, PIN, ASSISTED).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre amigable del metodo.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del metodo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si requiere contrasena.
        /// </summary>
        public bool RequiresPassword { get; set; }

        /// <summary>
        /// Indica si requiere PIN.
        /// </summary>
        public bool RequiresPin { get; set; }

        /// <summary>
        /// Indica si requiere supervisor.
        /// </summary>
        public bool RequiresSupervisor { get; set; }

        /// <summary>
        /// Orden de visualizacion.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
