namespace InclusiON.DTOs.Requests.Messages
{
    public class SendMessageRequest
    {
        /// <summary>ID del usuario destinatario.</summary>
        public Guid ReceiverId { get; set; }

        /// <summary>Asunto del mensaje. Opcional.</summary>
        public string? Subject { get; set; }

        /// <summary>Contenido del mensaje.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>ID de la persona con discapacidad a la que refiere el mensaje. Opcional.</summary>
        public Guid? RelatedPersonId { get; set; }
    }
}
