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

        /// <summary>"Professional" | "FamilyRepresentative" | "Admin"</summary>
        public string UserType { get; set; } = string.Empty;

        /// <summary>Fecha y hora del último mensaje intercambiado en la conversación.</summary>
        public DateTime? UltimoMensajeFecha { get; set; }

        /// <summary>Alias en inglés para compatibilidad.</summary>
        public DateTime? LastMessageDate => UltimoMensajeFecha;

        /// <summary>Cantidad de mensajes no leídos dirigidos al usuario actual.</summary>
        public int MensajesNoLeidos { get; set; }

        /// <summary>Alias en inglés para compatibilidad.</summary>
        public int UnreadCount => MensajesNoLeidos;
    }
}
