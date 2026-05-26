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
    }
}
