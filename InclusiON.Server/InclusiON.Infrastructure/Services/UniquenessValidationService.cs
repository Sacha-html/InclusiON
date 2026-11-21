using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;

namespace InclusiON.Infrastructure.Services
{
    public class UniquenessValidationService : IUniquenessValidationService
    {
        private readonly AppDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public UniquenessValidationService(AppDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<DocumentCheckResult> CheckDocumentAsync(
            string? documentNumber,
            Guid? excludeEntityId = null,
            CancellationToken cancellationToken = default)
        {
            var normalized = UniquenessNormalizer.NormalizeDocument(documentNumber);
            if (normalized is null)
            {
                return DocumentCheckResult.Available();
            }

            var normalizedUpper = normalized.ToUpperInvariant();

            // 1. Alumnos (PersonsWithDisability) — activos e inactivos
            var personQuery = _context.PersonsWithDisability
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.DocumentNumber != null && p.DocumentNumber.Trim().ToUpper() == normalizedUpper);

            if (excludeEntityId.HasValue)
            {
                personQuery = personQuery.Where(p => p.Id != excludeEntityId.Value);
            }

            var existingPerson = await personQuery.FirstOrDefaultAsync(cancellationToken);
            if (existingPerson is not null)
            {
                var isInactive = !existingPerson.IsActive || (existingPerson.User is not null && !existingPerson.User.IsActive);
                var message = isInactive
                    ? $"Ya existe un alumno inactivo registrado con el DNI {normalized}. Podés reactivarlo desde el listado de Alumnos."
                    : $"El DNI {normalized} ya se encuentra registrado en el sistema (Alumno).";

                return DocumentCheckResult.Taken("Alumno", isInactive, message);
            }

            // 2. Familiares / Tutores (FamilyRepresentatives)
            var familyQuery = _context.FamilyRepresentatives
                .AsNoTracking()
                .Where(f => f.DocumentNumber != null && f.DocumentNumber.Trim().ToUpper() == normalizedUpper);

            if (excludeEntityId.HasValue)
            {
                familyQuery = familyQuery.Where(f => f.Id != excludeEntityId.Value);
            }

            if (await familyQuery.AnyAsync(cancellationToken))
            {
                return DocumentCheckResult.Taken(
                    "Familiar",
                    false,
                    $"El DNI {normalized} ya se encuentra registrado en el sistema (Familiar).");
            }

            // 3. Profesionales (Professionals)
            var professionalQuery = _context.Professionals
                .AsNoTracking()
                .Where(p => p.DocumentNumber != null && p.DocumentNumber.Trim().ToUpper() == normalizedUpper);

            if (excludeEntityId.HasValue)
            {
                professionalQuery = professionalQuery.Where(p => p.Id != excludeEntityId.Value);
            }

            if (await professionalQuery.AnyAsync(cancellationToken))
            {
                return DocumentCheckResult.Taken(
                    "Profesional",
                    false,
                    $"El DNI {normalized} ya se encuentra registrado en el sistema (Profesional).");
            }

            return DocumentCheckResult.Available();
        }

        public async Task<EmailCheckResult> CheckEmailAsync(
            string? email,
            Guid? excludeUserId = null,
            Guid? excludeEntityId = null,
            bool checkPendingInvitations = false,
            CancellationToken cancellationToken = default)
        {
            var normalized = UniquenessNormalizer.NormalizeEmail(email);
            if (normalized is null)
            {
                return EmailCheckResult.Available();
            }

            var normalizedUpper = normalized.ToUpperInvariant();
            var effectiveExcludeUserId = excludeUserId;

            if (!effectiveExcludeUserId.HasValue && excludeEntityId.HasValue)
            {
                effectiveExcludeUserId = await ResolveUserIdFromEntityAsync(excludeEntityId.Value, cancellationToken);
            }

            // 1. Tabla Users (Administradores, Profesionales, Familiares, Alumnos)
            var userQuery = _context.Users
                .AsNoTracking()
                .Where(u =>
                    (u.NormalizedEmail != null && u.NormalizedEmail == normalizedUpper) ||
                    (u.Email != null && u.Email.Trim().ToLower() == normalized));

            if (effectiveExcludeUserId.HasValue)
            {
                userQuery = userQuery.Where(u => u.Id != effectiveExcludeUserId.Value);
            }

            if (await userQuery.AnyAsync(cancellationToken))
            {
                return EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema.");
            }

            // 2. Tabla Professionals (columna Email redundante en pendientes/registros)
            var professionalQuery = _context.Professionals
                .AsNoTracking()
                .Where(p => p.Email != null && p.Email.Trim().ToLower() == normalized);

            if (excludeEntityId.HasValue)
            {
                professionalQuery = professionalQuery.Where(p => p.Id != excludeEntityId.Value);
            }

            if (effectiveExcludeUserId.HasValue)
            {
                professionalQuery = professionalQuery.Where(p => p.UserId != effectiveExcludeUserId.Value);
            }

            if (await professionalQuery.AnyAsync(cancellationToken))
            {
                return EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema.");
            }

            // 3. Invitaciones pendientes activas (opcional, para evitar re-invitar al mismo correo)
            if (checkPendingInvitations)
            {
                var now = _dateTime.UtcNow;
                var hasActiveInvitation = await _context.Invitations
                    .AsNoTracking()
                    .AnyAsync(i =>
                        i.IsActive &&
                        !i.IsUsed &&
                        i.ExpiresAt > now &&
                        i.Email.Trim().ToLower() == normalized,
                        cancellationToken);

                if (hasActiveInvitation)
                {
                    return EmailCheckResult.Taken("Ya existe una invitación pendiente activa para este email.");
                }
            }

            return EmailCheckResult.Available();
        }

        public async Task<LicenseCheckResult> CheckLicenseNumberAsync(
            string? licenseNumber,
            Guid? excludeProfessionalId = null,
            CancellationToken cancellationToken = default)
        {
            var normalized = UniquenessNormalizer.NormalizeLicense(licenseNumber);
            if (normalized is null)
            {
                return LicenseCheckResult.Available();
            }

            var normalizedUpper = normalized.ToUpperInvariant();

            var query = _context.Professionals
                .AsNoTracking()
                .Where(p => p.LicenseNumber != null && p.LicenseNumber.Trim().ToUpper() == normalizedUpper);

            if (excludeProfessionalId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProfessionalId.Value);
            }

            if (await query.AnyAsync(cancellationToken))
            {
                return LicenseCheckResult.Taken("Esta matrícula ya se encuentra registrada en el sistema.");
            }

            return LicenseCheckResult.Available();
        }

        private async Task<Guid?> ResolveUserIdFromEntityAsync(Guid entityId, CancellationToken cancellationToken)
        {
            var familyUserId = await _context.FamilyRepresentatives
                .AsNoTracking()
                .Where(f => f.Id == entityId)
                .Select(f => (Guid?)f.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (familyUserId.HasValue) return familyUserId;

            var professionalUserId = await _context.Professionals
                .AsNoTracking()
                .Where(p => p.Id == entityId)
                .Select(p => (Guid?)p.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (professionalUserId.HasValue) return professionalUserId;

            return await _context.PersonsWithDisability
                .AsNoTracking()
                .Where(p => p.Id == entityId)
                .Select(p => (Guid?)p.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
