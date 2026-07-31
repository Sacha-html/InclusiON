using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Exceptions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

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

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public async Task<PersonWithDisability?> FindPersonByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;

            try
            {
                var rawPattern = $"%{identifier}%";
                var cleanPattern = $"%{RemoveDiacritics(identifier)}%";

                return await _context.PersonsWithDisability
                    .Include(p => p.User)
                    .Include(p => p.LoginMethod)
                    .Include(p => p.SupervisorUser)
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.User.IsActive &&
                        (EF.Functions.ILike(p.FirstName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName, cleanPattern) ||
                         EF.Functions.ILike(p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.LastName + " " + p.FirstName, rawPattern) ||
                         EF.Functions.ILike(p.LastName + " " + p.FirstName, cleanPattern) ||
                         EF.Functions.ILike(p.User.UserName!, rawPattern) ||
                         EF.Functions.ILike(p.User.UserName!, cleanPattern) ||
                         EF.Functions.ILike(p.User.Email!, rawPattern) ||
                         EF.Functions.ILike(p.User.Email!, cleanPattern)))
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding person by identifier: {Identifier}", identifier);
                throw new DataAccessException($"Error searching for person with identifier '{identifier}'", nameof(PersonWithDisability), ex);
            }
        }

        public async Task<IReadOnlyList<PersonWithDisability>> FindPersonsByIdentifierAsync(
            string identifier,
            int limit,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return Array.Empty<PersonWithDisability>();

            try
            {
                var rawPattern = $"%{identifier}%";
                var cleanPattern = $"%{RemoveDiacritics(identifier)}%";

                return await _context.PersonsWithDisability
                    .Include(p => p.User)
                    .Include(p => p.LoginMethod)
                    .Include(p => p.SupervisorUser)
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.User.IsActive &&
                        (EF.Functions.ILike(p.FirstName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName, cleanPattern) ||
                         EF.Functions.ILike(p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.LastName + " " + p.FirstName, rawPattern) ||
                         EF.Functions.ILike(p.LastName + " " + p.FirstName, cleanPattern) ||
                         EF.Functions.ILike(p.User.UserName!, rawPattern) ||
                         EF.Functions.ILike(p.User.UserName!, cleanPattern) ||
                         EF.Functions.ILike(p.User.Email!, rawPattern) ||
                         EF.Functions.ILike(p.User.Email!, cleanPattern)))
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Take(limit)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding persons by identifier: {Identifier}", identifier);
                throw new DataAccessException($"Error searching for persons with identifier '{identifier}'", nameof(PersonWithDisability), ex);
            }
        }

        public async Task<Professional?> FindProfessionalByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;

            try
            {
                var rawPattern = $"%{identifier}%";
                var cleanPattern = $"%{RemoveDiacritics(identifier)}%";

                return await _context.Professionals
                    .Include(p => p.User)
                    .AsNoTracking()
                    .Where(p => p.IsActive &&
                        (EF.Functions.ILike(p.FirstName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName, cleanPattern) ||
                         EF.Functions.ILike(p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, rawPattern) ||
                         EF.Functions.ILike(p.FirstName + " " + p.LastName, cleanPattern) ||
                         EF.Functions.ILike(p.User.UserName!, rawPattern) ||
                         EF.Functions.ILike(p.User.UserName!, cleanPattern) ||
                         EF.Functions.ILike(p.User.Email!, rawPattern) ||
                         EF.Functions.ILike(p.User.Email!, cleanPattern) ||
                         EF.Functions.ILike(p.LicenseNumber!, rawPattern)))
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
            if (string.IsNullOrWhiteSpace(identifier)) return null;

            try
            {
                var rawPattern = $"%{identifier}%";
                var cleanPattern = $"%{RemoveDiacritics(identifier)}%";

                return await _context.FamilyRepresentatives
                    .Include(f => f.User)
                    .AsNoTracking()
                    .Where(f => f.IsActive &&
                        (EF.Functions.ILike(f.FirstName, rawPattern) ||
                         EF.Functions.ILike(f.FirstName, cleanPattern) ||
                         EF.Functions.ILike(f.LastName, rawPattern) ||
                         EF.Functions.ILike(f.LastName, cleanPattern) ||
                         EF.Functions.ILike(f.FirstName + " " + f.LastName, rawPattern) ||
                         EF.Functions.ILike(f.FirstName + " " + f.LastName, cleanPattern) ||
                         EF.Functions.ILike(f.User.UserName!, rawPattern) ||
                         EF.Functions.ILike(f.User.UserName!, cleanPattern) ||
                         EF.Functions.ILike(f.User.Email!, rawPattern) ||
                         EF.Functions.ILike(f.User.Email!, cleanPattern)))
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
                    .AsNoTracking()
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
                    .AsNoTracking()
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
                    .AsNoTracking()
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
                    .AsNoTracking()
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
                    .AsNoTracking()
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
                    .AsNoTracking()
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

        public Task<bool> CanProfessionalSupervisedLoginAsync(
            Guid professionalId,
            Guid personId,
            CancellationToken cancellationToken = default)
        {
            return _context.ProfessionalPersons
                .AnyAsync(pp => pp.ProfessionalId == professionalId
                             && pp.PersonId == personId
                             && pp.IsActive
                             && pp.CanSuperviseLogin,
                    cancellationToken);
        }

        public Task<bool> CanFamilySupervisedLoginAsync(
            Guid familyRepresentativeId,
            Guid personId,
            CancellationToken cancellationToken = default)
        {
            return _context.PersonRepresentatives
                .AnyAsync(pr => pr.RepresentativeId == familyRepresentativeId
                             && pr.PersonId == personId
                             && pr.IsActive
                             && pr.CanSuperviseLogin,
                    cancellationToken);
        }
    }
}
