using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.Interfaces.Repositories
{
    public interface IUsersRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(User user);
        //Task<bool> UpdateLastLoginAsync(Guid userId, DateTime lastLoginDate);
    }
}
