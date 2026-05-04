using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    /// <summary>
    /// Mappers internos reutilizados por todos los handlers de mensajería.
    /// </summary>
    internal static class MessageMapper
    {
        private const int PreviewLength = 150;

        internal static string FullName(User user) =>
            string.IsNullOrWhiteSpace(user.Name) && string.IsNullOrWhiteSpace(user.Surname)
                ? user.Email ?? string.Empty
                : $"{user.Name} {user.Surname}".Trim();

        internal static MessageListItemResponse ToListItem(Message m) => new()
        {
            Id              = m.Id,
            Subject         = m.Subject,
            ContentPreview  = m.Content.Length <= PreviewLength
                                  ? m.Content
                                  : m.Content[..PreviewLength] + "…",
            SentAt          = m.SentAt,
            ReadAt          = m.ReadAt,
            IsRead          = m.IsRead,
            SenderId        = m.SenderId,
            SenderFullName  = FullName(m.Sender),
            ReceiverId      = m.ReceiverId,
            ReceiverFullName = FullName(m.Receiver),
            RelatedPersonId = m.RelatedPersonId,
            ParentMessageId = m.ParentMessageId,
            ReplyCount      = m.Replies?.Count ?? 0
        };

        internal static MessageResponse ToDetail(Message m) => new()
        {
            Id              = m.Id,
            Subject         = m.Subject,
            Content         = m.Content,
            SentAt          = m.SentAt,
            ReadAt          = m.ReadAt,
            IsRead          = m.IsRead,
            SenderId        = m.SenderId,
            SenderFullName  = FullName(m.Sender),
            ReceiverId      = m.ReceiverId,
            ReceiverFullName = FullName(m.Receiver),
            RelatedPersonId = m.RelatedPersonId,
            ParentMessageId = m.ParentMessageId,
            Replies         = m.Replies
                .OrderBy(r => r.SentAt)
                .Select(ToListItem)
                .ToList()
        };
    }
}
