namespace InclusiON.DTOs.Responses.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool MustChangePassword { get; set; }
        public UserResponse User { get; set; } = new();

        /// <summary>
        /// Preferencias de accesibilidad del usuario (si tiene perfil de persona asociado).
        /// </summary>
        public AccessibilityPreferences? Accessibility { get; set; }
    }
}
