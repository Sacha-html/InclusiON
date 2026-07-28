namespace InclusiON.DTOs.Responses.Messages
{
    /// <summary>
    /// Contacto disponible para mensajería (usuario con quien se puede intercambiar mensajes).
    /// </summary>
    public class MessageContactResponse
    {
        public Guid   UserId   { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;

        /// <summary>"Professional" | "FamilyRepresentative"</summary>
        public string UserType { get; set; } = string.Empty;
    }
}
