namespace InclusiON.DTOs.Responses.Messages
{
    /// <summary>
    /// Resumen de un mensaje para listados (bandeja de entrada / enviados).
    /// </summary>
    public class MessageListItemResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;

        /// <summary>Asunto del mensaje.</summary>
        public string? Subject { get; set; }

        /// <summary>Primeros 150 caracteres del contenido.</summary>
        public string ContentPreview { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }

        public Guid SenderId { get; set; }
        public string SenderFullName { get; set; } = string.Empty;

        public Guid ReceiverId { get; set; }
        public string ReceiverFullName { get; set; } = string.Empty;

        /// <summary>Persona con discapacidad relacionada. Null si el mensaje no refiere a ninguna.</summary>
        public Guid? RelatedPersonId { get; set; }

        /// <summary>ID del mensaje padre si es una respuesta.</summary>
        public int? ParentMessageId { get; set; }

        /// <summary>Cantidad de respuestas directas a este mensaje.</summary>
        public int ReplyCount { get; set; }
    }
}
