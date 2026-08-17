using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly AppDbContext _context;

        public MessagesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Message> Items, int Total)> GetInboxAsync(
            Guid userId, int page, int pageSize,
            bool? isRead = null,
            Guid? relatedPersonId = null,
            Guid? senderId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                .Where(m => m.ReceiverId == userId
                         && m.IsActive
                         && m.ParentMessageId == null)
                .AsNoTracking();

            if (isRead.HasValue)
                query = query.Where(m => m.IsRead == isRead.Value);

            if (relatedPersonId.HasValue)
                query = query.Where(m => m.RelatedPersonId == relatedPersonId.Value);

            if (senderId.HasValue)
                query = query.Where(m => m.SenderId == senderId.Value);

            var paged = await query
                .OrderByDescending(m => !m.IsRead)
                .ThenByDescending(m => m.SentAt)
                .ToPagedAsync(page, pageSize, cancellationToken);

            return (paged.Data, paged.TotalRecords);
        }

        public async Task<(List<Message> Items, int Total)> GetSentAsync(
            Guid userId, int page, int pageSize,
            bool? isRead = null,
            Guid? relatedPersonId = null,
            Guid? receiverId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                .Where(m => m.SenderId == userId
                         && m.IsActive
                         && m.ParentMessageId == null)
                .AsNoTracking();

            if (isRead.HasValue)
                query = query.Where(m => m.IsRead == isRead.Value);

            if (relatedPersonId.HasValue)
                query = query.Where(m => m.RelatedPersonId == relatedPersonId.Value);

            if (receiverId.HasValue)
                query = query.Where(m => m.ReceiverId == receiverId.Value);

            var paged = await query
                .OrderByDescending(m => m.SentAt)
                .ToPagedAsync(page, pageSize, cancellationToken);

            return (paged.Data, paged.TotalRecords);
        }

        public async Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Sender)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Receiver)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && m.IsActive && !m.IsRead, cancellationToken);
        }

        public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
            return message;
        }

        public async Task<Dictionary<Guid, (DateTime? LastMessageDate, int UnreadCount)>> GetConversationStatsAsync(
            Guid currentUserId,
            IEnumerable<Guid> contactUserIds,
            CancellationToken cancellationToken = default)
        {
            var contactIdList = contactUserIds.Distinct().ToList();
            if (contactIdList.Count == 0)
                return new Dictionary<Guid, (DateTime?, int)>();

            var stats = await _context.Messages
                .Where(m => m.IsActive &&
                            ((m.SenderId == currentUserId && contactIdList.Contains(m.ReceiverId)) ||
                             (m.ReceiverId == currentUserId && contactIdList.Contains(m.SenderId))))
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    ContactId = g.Key,
                    LastMessageDate = g.Max(m => (DateTime?)m.SentAt),
                    UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                })
                .ToListAsync(cancellationToken);

            return stats.ToDictionary(s => s.ContactId, s => (s.LastMessageDate, s.UnreadCount));
        }

        public async Task<int> MarkConversationAsReadAsync(
            Guid currentUserId,
            Guid contactUserId,
            CancellationToken cancellationToken = default)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.IsActive && !m.IsRead && m.ReceiverId == currentUserId && m.SenderId == contactUserId)
                .ToListAsync(cancellationToken);

            if (unreadMessages.Count == 0)
                return 0;

            var now = DateTime.UtcNow;
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = now;
            }

            return unreadMessages.Count;
        }
    }
}
