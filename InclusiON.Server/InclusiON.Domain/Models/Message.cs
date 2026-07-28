using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Mensaje de comunicacion entre usuarios del sistema.
    /// Permite comunicacion entre profesionales y familiares sobre una persona.
    /// </summary>
    public class Message : IdentifiableEntity
    {
        /// <summary>
        /// ID del usuario que envia el mensaje.
        /// </summary>
        public Guid SenderId { get; set; }

        /// <summary>
        /// ID del usuario que recibe el mensaje.
        /// </summary>
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad relacionada al mensaje (opcional).
        /// </summary>
        public Guid? RelatedPersonId { get; set; }

        /// <summary>
        /// Asunto del mensaje.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Contenido del mensaje.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de envio.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora en que fue leido.
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Indica si el mensaje fue leido.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// ID del mensaje padre si es una respuesta.
        /// </summary>
        public int? ParentMessageId { get; set; }

        /// <summary>
        /// Indica si el mensaje esta activo.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Usuario que envia el mensaje.
        /// </summary>
        public virtual User Sender { get; set; } = null!;

        /// <summary>
        /// Usuario que recibe el mensaje.
        /// </summary>
        public virtual User Receiver { get; set; } = null!;

        /// <summary>
        /// Persona con discapacidad relacionada.
        /// </summary>
        public virtual PersonWithDisability? RelatedPerson { get; set; }

        /// <summary>
        /// Mensaje padre si es una respuesta.
        /// </summary>
        public virtual Message? ParentMessage { get; set; }

        /// <summary>
        /// Respuestas a este mensaje.
        /// </summary>
        public virtual ICollection<Message> Replies { get; set; }

        public Message()
        {
            Replies = new HashSet<Message>();
        }
    }
}
