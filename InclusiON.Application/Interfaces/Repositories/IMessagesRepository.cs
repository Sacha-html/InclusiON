using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de mensajería entre usuarios.
    /// </summary>
    public interface IMessagesRepository
    {
        /// <summary>
        /// Obtiene la bandeja de entrada paginada de un usuario (mensajes recibidos, activos).
        /// Solo mensajes de nivel superior (no replies).
        /// Orden: no leídos primero, luego SentAt DESC.
        /// </summary>
        Task<(List<Message> Items, int Total)> GetInboxAsync(
            Guid userId, int skip, int take,
            bool? isRead = null,
            Guid? relatedPersonId = null,
            Guid? senderId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los mensajes enviados paginados de un usuario (activos).
        /// Solo mensajes de nivel superior (no replies).
        /// Ordenados por SentAt DESC.
        /// </summary>
        Task<(List<Message> Items, int Total)> GetSentAsync(
            Guid userId, int skip, int take,
            bool? isRead = null,
            Guid? relatedPersonId = null,
            Guid? receiverId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un mensaje por su ID, incluyendo sender, receiver y respuestas directas.
        /// </summary>
        Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cantidad de mensajes no leídos en la bandeja de entrada del usuario.
        /// </summary>
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persiste un nuevo mensaje en la base de datos.
        /// </summary>
        Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);
    }
}
