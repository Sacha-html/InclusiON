namespace InclusiON.DTOs.Responses.Messages
{
    /// <summary>
    /// Detalle completo de un mensaje, incluyendo respuestas directas.
    /// </summary>
    public class MessageResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;

        public string? Subject { get; set; }
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }

        public Guid SenderId { get; set; }
        public string SenderFullName { get; set; } = string.Empty;

        public Guid ReceiverId { get; set; }
        public string ReceiverFullName { get; set; } = string.Empty;

        /// <summary>Persona con discapacidad relacionada. Null si no aplica.</summary>
        public Guid? RelatedPersonId { get; set; }

        /// <summary>ID del mensaje padre si es una respuesta.</summary>
        public int? ParentMessageId { get; set; }

        /// <summary>Respuestas directas a este mensaje, ordenadas por SentAt ASC.</summary>
        public List<MessageReplyResponse> Replies { get; set; } = new();
    }
}
