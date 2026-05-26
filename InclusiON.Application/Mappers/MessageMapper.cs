using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.Mappers
{
    public static class MessageMapper
    {
        private const int PreviewLength = 150;

        public static string FullName(User user) =>
            string.IsNullOrWhiteSpace(user.Name) && string.IsNullOrWhiteSpace(user.Surname)
                ? user.Email ?? string.Empty
                : $"{user.Name} {user.Surname}".Trim();

        public static MessageContactResponse ToContactResponse(User u, string userType) => new()
        {
            UserId   = u.Id,
            FullName = FullName(u),
            Email    = u.Email ?? string.Empty,
            UserType = userType,
        };

        public static MessageListItemResponse ToListItem(Message m) => new()
        {
            Id               = m.Id,
            Subject          = m.Subject,
            ContentPreview   = m.Content.Length <= PreviewLength
                                   ? m.Content
                                   : m.Content[..PreviewLength] + "…",
            SentAt           = m.SentAt,
            ReadAt           = m.ReadAt,
            IsRead           = m.IsRead,
            SenderId         = m.SenderId,
            SenderFullName   = FullName(m.Sender),
            ReceiverId       = m.ReceiverId,
            ReceiverFullName = FullName(m.Receiver),
            RelatedPersonId  = m.RelatedPersonId,
            ParentMessageId  = m.ParentMessageId,
            ReplyCount       = m.Replies?.Count ?? 0,
        };

        public static MessageReplyResponse ToReply(Message m) => new()
        {
            Id               = m.Id,
            Subject          = m.Subject,
            Content          = m.Content,
            SentAt           = m.SentAt,
            ReadAt           = m.ReadAt,
            IsRead           = m.IsRead,
            SenderId         = m.SenderId,
            SenderFullName   = FullName(m.Sender),
            ReceiverId       = m.ReceiverId,
            ReceiverFullName = FullName(m.Receiver),
            RelatedPersonId  = m.RelatedPersonId,
            ParentMessageId  = m.ParentMessageId,
        };

        public static MessageResponse ToDetail(Message m) => new()
        {
            Id               = m.Id,
            Subject          = m.Subject,
            Content          = m.Content,
            SentAt           = m.SentAt,
            ReadAt           = m.ReadAt,
            IsRead           = m.IsRead,
            SenderId         = m.SenderId,
            SenderFullName   = FullName(m.Sender),
            ReceiverId       = m.ReceiverId,
            ReceiverFullName = FullName(m.Receiver),
            RelatedPersonId  = m.RelatedPersonId,
            ParentMessageId  = m.ParentMessageId,
            Replies          = m.Replies
                .OrderBy(r => r.SentAt)
                .Select(ToReply)
                .ToList(),
        };
    }
}
