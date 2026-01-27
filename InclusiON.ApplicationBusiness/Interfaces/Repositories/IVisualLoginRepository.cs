using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de login visual.
    /// </summary>
    public interface IVisualLoginRepository
    {
        /// <summary>
        /// Busca una persona con discapacidad por identificador (nombre, username o email).
        /// </summary>
        Task<PersonWithDisability?> FindPersonByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca un profesional por identificador.
        /// </summary>
        Task<Professional?> FindProfessionalByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca un familiar por identificador.
        /// </summary>
        Task<FamilyRepresentative?> FindFamilyByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca una persona con discapacidad por UserId.
        /// </summary>
        Task<PersonWithDisability?> GetPersonByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca un profesional por UserId.
        /// </summary>
        Task<Professional?> GetProfessionalByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca un familiar por UserId.
        /// </summary>
        Task<FamilyRepresentative?> GetFamilyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un dispositivo es confiable para un usuario.
        /// </summary>
        Task<bool> IsTrustedDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un dispositivo confiable por UserId y DeviceId.
        /// </summary>
        Task<TrustedDevice?> GetTrustedDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra o actualiza un dispositivo confiable.
        /// </summary>
        Task RegisterTrustedDeviceAsync(TrustedDevice device, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza la fecha de ultimo uso de un dispositivo confiable.
        /// </summary>
        Task UpdateDeviceLastUsedAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los metodos de login activos.
        /// </summary>
        Task<IEnumerable<LoginMethod>> GetActiveLoginMethodsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un metodo de login por ID.
        /// </summary>
        Task<LoginMethod?> GetLoginMethodByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza el metodo de login de una persona.
        /// </summary>
        Task UpdatePersonLoginMethodAsync(Guid userId, int loginMethodId, string? pinHash, Guid? supervisorUserId, CancellationToken cancellationToken = default);
    }
}
