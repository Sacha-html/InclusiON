namespace InclusiON.DTOs.Responses.Auth
{
    /// <summary>
    /// Respuesta comun para todos los metodos de login visual.
    /// </summary>
    public class VisualLoginResponse
    {
        /// <summary>
        /// Indica si el login fue exitoso.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Token JWT para autenticacion de sesion.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token de refresco para renovar la sesion.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Fecha de expiracion del token.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Informacion basica del usuario autenticado.
        /// </summary>
        public VisualLoginUserInfo? User { get; set; }

        /// <summary>
        /// Mensaje de error si el login fallo.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Numero de intentos restantes antes de bloqueo (si aplica).
        /// </summary>
        public int? RemainingAttempts { get; set; }

        /// <summary>
        /// Indica si la cuenta esta bloqueada por intentos fallidos.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Tiempo restante de bloqueo en segundos.
        /// </summary>
        public int? LockoutSecondsRemaining { get; set; }
    }

    /// <summary>
    /// Informacion basica del usuario autenticado para login visual.
    /// </summary>
    public class VisualLoginUserInfo
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre para mostrar.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Inicial para el avatar.
        /// </summary>
        public string Initial { get; set; } = string.Empty;

        /// <summary>
        /// Color del avatar.
        /// </summary>
        public string AvatarColor { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de usuario (Person, Professional, Family).
        /// </summary>
        public string UserType { get; set; } = string.Empty;

        /// <summary>
        /// Roles asignados al usuario.
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Preferencias de accesibilidad.
        /// </summary>
        public AccessibilityPreferences? Accessibility { get; set; }
    }

    /// <summary>
    /// Preferencias de accesibilidad del usuario.
    /// </summary>
    public class AccessibilityPreferences
    {
        /// <summary>
        /// Requiere fuente grande.
        /// </summary>
        public bool RequiresLargeFont { get; set; }

        /// <summary>
        /// Requiere alto contraste.
        /// </summary>
        public bool RequiresHighContrast { get; set; }

        /// <summary>
        /// Sensibilidad al ruido visual.
        /// </summary>
        public bool VisualNoiseSensitivity { get; set; }

        /// <summary>
        /// Sensibilidad al sonido.
        /// </summary>
        public bool SoundSensitivity { get; set; }
    }
}
