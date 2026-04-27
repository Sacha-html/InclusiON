namespace InclusiON.DTOs.Responses.Auth
{
    /// <summary>
    /// Resumen de un usuario candidato cuando el identificador matchea más de uno.
    /// Permite al frontend mostrar una lista para que la persona elija cuál es ella.
    /// </summary>
    public class UserMatchSummary
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Initial { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = string.Empty;
        public string LoginMethodCode { get; set; } = string.Empty;
        public string LoginMethodName { get; set; } = string.Empty;
        public bool RequiresSupervision { get; set; }
        public bool IsTrustedDevice { get; set; }

        /// <summary>
        /// Inicial del apellido (privacidad: no exponemos el apellido completo en la lista).
        /// </summary>
        public string? LastNameInitial { get; set; }
    }
}
