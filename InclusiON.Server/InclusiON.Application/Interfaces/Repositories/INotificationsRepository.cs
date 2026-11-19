using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories;

public interface INotificationsRepository
{
    Task AddRangeAndSaveAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 30, bool onlyUnread = false, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, DateTime readAt, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAt, CancellationToken cancellationToken = default);
    Task<bool> DismissAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> DismissAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetUserIdsInRoleAsync(IEnumerable<Guid> userIds, string normalizedRoleName, CancellationToken cancellationToken = default);
}
