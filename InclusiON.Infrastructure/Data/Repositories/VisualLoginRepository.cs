using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                var lowerIdentifier = identifier.ToLower();
                return await _context.PersonsWithDisability
                    .Include(p => p.User)
                    .Include(p => p.LoginMethod)
                    .Where(p => p.IsActive &&
                        (p.FirstName.ToLower().Contains(lowerIdentifier) ||
                         p.LastName.ToLower().Contains(lowerIdentifier) ||
                         (p.FirstName + " " + p.LastName).ToLower().Contains(lowerIdentifier) ||
                         p.User.UserName!.ToLower() == lowerIdentifier ||
                         p.User.Email!.ToLower() == lowerIdentifier))
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding person by identifier: {Identifier}", identifier);
                return null;
            }
        }

        public async Task<Professional?> FindProfessionalByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var lowerIdentifier = identifier.ToLower();
                return await _context.Professionals
                    .Include(p => p.User)
                    .Where(p => p.IsActive &&
                        (p.FirstName.ToLower().Contains(lowerIdentifier) ||
                         p.LastName.ToLower().Contains(lowerIdentifier) ||
                         (p.FirstName + " " + p.LastName).ToLower().Contains(lowerIdentifier) ||
                         p.User.UserName!.ToLower() == lowerIdentifier ||
                         p.User.Email!.ToLower() == lowerIdentifier ||
                         p.LicenseNumber!.ToLower() == lowerIdentifier))
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding professional by identifier: {Identifier}", identifier);
                return null;
            }
        }

        public async Task<FamilyRepresentative?> FindFamilyByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var lowerIdentifier = identifier.ToLower();
                return await _context.FamilyRepresentatives
                    .Include(f => f.User)
                    .Where(f => f.IsActive &&
                        (f.FirstName.ToLower().Contains(lowerIdentifier) ||
                         f.LastName.ToLower().Contains(lowerIdentifier) ||
                         (f.FirstName + " " + f.LastName).ToLower().Contains(lowerIdentifier) ||
                         f.User.UserName!.ToLower() == lowerIdentifier ||
                         f.User.Email!.ToLower() == lowerIdentifier))
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding family by identifier: {Identifier}", identifier);
                return null;
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
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting person by user ID: {UserId}", userId);
                return null;
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
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting professional by user ID: {UserId}", userId);
                return null;
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
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting family by user ID: {UserId}", userId);
                return null;
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
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking trusted device for user: {UserId}", userId);
                return false;
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
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trusted device for user: {UserId}", userId);
                return null;
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
                        cancellationToken);

                if (existing != null)
                {
                    existing.LastUsedAt = DateTime.UtcNow;
                    existing.ExpiresAt = DateTime.UtcNow.AddDays(90);
                    existing.IsActive = true;
                }
                else
                {
                    await _context.TrustedDevices.AddAsync(device, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Trusted device registered for user: {UserId}", device.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering trusted device for user: {UserId}", device.UserId);
                throw;
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
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device last used: {DeviceId}", deviceId);
                throw;
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
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active login methods");
                return Enumerable.Empty<LoginMethod>();
            }
        }

        public async Task<LoginMethod?> GetLoginMethodByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.LoginMethods
                    .FirstOrDefaultAsync(lm => lm.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login method by ID: {Id}", id);
                return null;
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
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                if (person == null)
                {
                    throw new InvalidOperationException($"Person with UserId {userId} not found");
                }

                person.LoginMethodId = loginMethodId;
                person.PinCodeHash = pinHash;
                person.SupervisorUserId = supervisorUserId;
                person.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Updated login method for user: {UserId} to method: {LoginMethodId}", userId, loginMethodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login method for user: {UserId}", userId);
                throw;
            }
        }
    }
}
