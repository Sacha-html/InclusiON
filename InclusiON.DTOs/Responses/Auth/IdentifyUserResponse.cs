namespace InclusiON.DTOs.Responses.Auth
{
    /// <summary>
    /// Respuesta al identificar un usuario para login.
    /// Contiene informacion sobre el metodo de login configurado.
    /// </summary>
    public class IdentifyUserResponse
    {
        /// <summary>
        /// Indica si el usuario fue encontrado.
        /// </summary>
        public bool UserFound { get; set; }

        /// <summary>
        /// ID del usuario (solo si fue encontrado).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Nombre para mostrar del usuario.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Inicial del nombre para el avatar.
        /// </summary>
        public string? Initial { get; set; }

        /// <summary>
        /// Color del avatar del usuario.
        /// </summary>
        public string? AvatarColor { get; set; }

        /// <summary>
        /// Codigo del metodo de login configurado (STANDARD, PIN, EMOJI_SEQUENCE, etc.).
        /// </summary>
        public string? LoginMethodCode { get; set; }

        /// <summary>
        /// Nombre amigable del metodo de login.
        /// </summary>
        public string? LoginMethodName { get; set; }

        /// <summary>
        /// Indica si el dispositivo actual es confiable para este usuario.
        /// </summary>
        public bool IsTrustedDevice { get; set; }

        /// <summary>
        /// Indica si el usuario requiere supervision para login.
        /// </summary>
        public bool RequiresSupervision { get; set; }

        /// <summary>
        /// Tipo de usuario (Person, Professional, Family).
        /// </summary>
        public string? UserType { get; set; }

        /// <summary>
        /// Mensaje de error si el usuario no fue encontrado.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
