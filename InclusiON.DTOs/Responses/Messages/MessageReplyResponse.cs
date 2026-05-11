namespace InclusiON.DTOs.Responses.Messages
{
    /// <summary>
    /// Respuesta directa a un mensaje, incluida en el detalle completo.
    /// Contiene el contenido completo (no preview) ya que es visible en el hilo de conversación.
    /// </summary>
    public class MessageReplyResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;

        public string? Subject { get; set; }

        /// <summary>Contenido completo de la respuesta.</summary>
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }

        public Guid SenderId { get; set; }
        public string SenderFullName { get; set; } = string.Empty;

        public Guid ReceiverId { get; set; }
        public string ReceiverFullName { get; set; } = string.Empty;

        public Guid? RelatedPersonId { get; set; }
        public int? ParentMessageId { get; set; }
    }
}
