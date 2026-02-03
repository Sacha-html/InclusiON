using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Exceptions;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Entities.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de login visual.
    /// </summary>
    public class VisualLoginRepository : IVisualLoginRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VisualLoginRepository> _logger;

        public VisualLoginRepository(AppDbContext context, ILogger<VisualLoginRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PersonWithDisability?> FindPersonByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar patrón LIKE para búsqueda eficiente (case-insensitive en SQL Server por defecto)
                var searchPattern = $"%{identifier}%";
                return await _context.PersonsWithDisability
                    .Include(p => p.User)
                    .Include(p => p.LoginMethod)
                    .Include(p => p.SupervisorUser)
                    .Where(p => p.IsActive &&
                        (EF.Functions.Like(p.FirstName, searchPattern) ||
                         EF.Functions.Like(p.LastName, searchPattern) ||
                         EF.Functions.Like(p.FirstName + " " + p.LastName, searchPattern) ||
                         EF.Functions.Like(p.User.UserName!, identifier) ||
                         EF.Functions.Like(p.User.Email!, identifier)))
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding person by identifier: {Identifier}", identifier);
                throw new DataAccessException($"Error searching for person with identifier '{identifier}'", nameof(PersonWithDisability), ex);
            }
        }

        public async Task<Professional?> FindProfessionalByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar patrón LIKE para búsqueda eficiente (case-insensitive en SQL Server por defecto)
                var searchPattern = $"%{identifier}%";
                return await _context.Professionals
                    .Include(p => p.User)
                    .Where(p => p.IsActive &&
                        (EF.Functions.Like(p.FirstName, searchPattern) ||
                         EF.Functions.Like(p.LastName, searchPattern) ||
                         EF.Functions.Like(p.FirstName + " " + p.LastName, searchPattern) ||
                         EF.Functions.Like(p.User.UserName!, identifier) ||
                         EF.Functions.Like(p.User.Email!, identifier) ||
                         EF.Functions.Like(p.LicenseNumber!, identifier)))
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding professional by identifier: {Identifier}", identifier);
                throw new DataAccessException($"Error searching for professional with identifier '{identifier}'", nameof(Professional), ex);
            }
        }

        public async Task<FamilyRepresentative?> FindFamilyByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar patrón LIKE para búsqueda eficiente (case-insensitive en SQL Server por defecto)
                var searchPattern = $"%{identifier}%";
                return await _context.FamilyRepresentatives
                    .Include(f => f.User)
                    .Where(f => f.IsActive &&
                        (EF.Functions.Like(f.FirstName, searchPattern) ||
                         EF.Functions.Like(f.LastName, searchPattern) ||
                         EF.Functions.Like(f.FirstName + " " + f.LastName, searchPattern) ||
                         EF.Functions.Like(f.User.UserName!, identifier) ||
                         EF.Functions.Like(f.User.Email!, identifier)))
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding family by identifier: {Identifier}", identifier);
                throw new DataAccessException($"Error searching for family representative with identifier '{identifier}'", nameof(FamilyRepresentative), ex);
            }
        }

        public async Task<PersonWithDisability?> GetPersonByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.PersonsWithDisability
                    .Include(p => p.User)
                    .Include(p => p.LoginMethod)
                    .Include(p => p.SupervisorUser)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting person by user ID: {UserId}", userId);
                throw new DataAccessException($"Error retrieving person with user ID '{userId}'", nameof(PersonWithDisability), ex);
            }
        }

        public async Task<Professional?> GetProfessionalByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Professionals
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting professional by user ID: {UserId}", userId);
                throw new DataAccessException($"Error retrieving professional with user ID '{userId}'", nameof(Professional), ex);
            }
        }

        public async Task<FamilyRepresentative?> GetFamilyByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.FamilyRepresentatives
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting family by user ID: {UserId}", userId);
                throw new DataAccessException($"Error retrieving family representative with user ID '{userId}'", nameof(FamilyRepresentative), ex);
            }
        }

        public async Task<bool> IsTrustedDeviceAsync(
            Guid userId,
            string deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.TrustedDevices
                    .AnyAsync(td => td.UserId == userId &&
                                   td.DeviceId == deviceId &&
                                   td.IsActive &&
                                   (td.ExpiresAt == null || td.ExpiresAt > DateTime.UtcNow),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking trusted device for user: {UserId}", userId);
                throw new DataAccessException($"Error checking trusted device for user '{userId}'", nameof(TrustedDevice), ex);
            }
        }

        public async Task<TrustedDevice?> GetTrustedDeviceAsync(
            Guid userId,
            string deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.TrustedDevices
                    .Include(td => td.User)
                    .FirstOrDefaultAsync(td => td.UserId == userId &&
                                               td.DeviceId == deviceId &&
                                               td.IsActive &&
                                               (td.ExpiresAt == null || td.ExpiresAt > DateTime.UtcNow),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trusted device for user: {UserId}", userId);
                throw new DataAccessException($"Error retrieving trusted device for user '{userId}'", nameof(TrustedDevice), ex);
            }
        }

        public async Task RegisterTrustedDeviceAsync(
            TrustedDevice device,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var existing = await _context.TrustedDevices
                    .FirstOrDefaultAsync(td => td.UserId == device.UserId &&
                                               td.DeviceId == device.DeviceId,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (existing != null)
                {
                    existing.LastUsedAt = DateTime.UtcNow;
                    existing.ExpiresAt = DateTime.UtcNow.AddDays(90);
                    existing.IsActive = true;
                }
                else
                {
                    await _context.TrustedDevices.AddAsync(device, cancellationToken).ConfigureAwait(false);
                }

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Trusted device registered for user: {UserId}", device.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering trusted device for user: {UserId}", device.UserId);
                throw new DataAccessException($"Error registering trusted device for user '{device.UserId}'", nameof(TrustedDevice), ex);
            }
        }

        public async Task UpdateDeviceLastUsedAsync(
            int deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.TrustedDevices
                    .Where(td => td.Id == deviceId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(td => td.LastUsedAt, DateTime.UtcNow),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device last used: {DeviceId}", deviceId);
                throw new DataAccessException($"Error updating device '{deviceId}' last used time", nameof(TrustedDevice), ex);
            }
        }

        public async Task<IEnumerable<LoginMethod>> GetActiveLoginMethodsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.LoginMethods
                    .Where(lm => lm.IsActive)
                    .OrderBy(lm => lm.DisplayOrder)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active login methods");
                throw new DataAccessException("Error retrieving active login methods", nameof(LoginMethod), ex);
            }
        }

        public async Task<LoginMethod?> GetLoginMethodByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.LoginMethods
                    .FirstOrDefaultAsync(lm => lm.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login method by ID: {Id}", id);
                throw new DataAccessException($"Error retrieving login method with ID '{id}'", nameof(LoginMethod), ex);
            }
        }

        public async Task UpdatePersonLoginMethodAsync(
            Guid userId,
            int loginMethodId,
            string? pinHash,
            Guid? supervisorUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var person = await _context.PersonsWithDisability
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
                    .ConfigureAwait(false);

                if (person == null)
                {
                    throw new EntityNotFoundException(nameof(PersonWithDisability), userId);
                }

                person.LoginMethodId = loginMethodId;
                person.PinCodeHash = pinHash;
                person.SupervisorUserId = supervisorUserId;
                person.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Updated login method for user: {UserId} to method: {LoginMethodId}", userId, loginMethodId);
            }
            catch (EntityNotFoundException)
            {
                throw; // Re-throw without wrapping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login method for user: {UserId}", userId);
                throw new DataAccessException($"Error updating login method for user '{userId}'", nameof(PersonWithDisability), ex);
            }
        }
    }
}
