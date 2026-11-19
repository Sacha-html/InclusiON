using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories;

public class NotificationsRepository : INotificationsRepository
{
    private readonly AppDbContext _context;

    public NotificationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAndSaveAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        var list = notifications.ToList();
        if (list.Count == 0) return;

        await _context.Notifications.AddRangeAsync(list, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Notification>> GetByUserIdAsync(
        Guid userId,
        int page = 1,
        int pageSize = 30,
        bool onlyUnread = false,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDismissed);

        if (onlyUnread)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsDismissed && !n.IsRead, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDismissed, cancellationToken);

        if (notif is null) return false;

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            notif.ReadAt = readAt;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDismissed && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0) return 0;

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = readAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }

    public async Task<bool> DismissAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDismissed, cancellationToken);

        if (notif is null) return false;

        notif.IsDismissed = true;
        notif.IsRead = true;
        notif.ReadAt ??= DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> DismissAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var active = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDismissed)
            .ToListAsync(cancellationToken);

        if (active.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var n in active)
        {
            n.IsDismissed = true;
            n.IsRead = true;
            n.ReadAt ??= now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return active.Count;
    }

    public async Task<HashSet<Guid>> GetUserIdsInRoleAsync(
        IEnumerable<Guid> userIds,
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new HashSet<Guid>();

        var roleId = await _context.Roles
            .AsNoTracking()
            .Where(r => r.NormalizedName == normalizedRoleName)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!roleId.HasValue) return new HashSet<Guid>();

        var matching = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId.Value && ids.Contains(ur.UserId))
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        return matching.ToHashSet();
    }
}
