using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IUsersRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario con sus navigations de perfil cargadas
        /// (Professional, FamilyRepresentative, PersonWithDisability).
        /// Permite determinar el tipo de usuario para validaciones de mensajería.
        /// </summary>
        Task<User?> GetByIdWithProfileAsync(Guid id, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(User user);
        //Task<bool> UpdateLastLoginAsync(Guid userId, DateTime lastLoginDate);
    }
}
