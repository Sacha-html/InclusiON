using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InclusiON.Entities.Models;
using InclusiON.Entities.Models.BaseEntities;
using System.Security.Claims;

namespace InclusiON.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // DbSets - Nivel 1 (Base)
        public DbSet<DisabilityType> DisabilityTypes { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<EducationalInstitution> EducationalInstitutions { get; set; }
        public DbSet<AutonomyLevel> AutonomyLevels { get; set; }
        public DbSet<LoginMethod> LoginMethods { get; set; }

        // DbSets - Nivel 2 (Perfiles)
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Professional> Professionals { get; set; }
        public DbSet<PersonWithDisability> PersonsWithDisability { get; set; }
        public DbSet<FamilyRepresentative> FamilyRepresentatives { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        // DbSets - Nivel 3 (Relaciones)
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<ProfessionalInstitution> ProfessionalInstitutions { get; set; }
        public DbSet<ProfessionalPerson> ProfessionalPersons { get; set; }
        public DbSet<PersonRepresentative> PersonRepresentatives { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<Activity> Activities { get; set; }

        // DbSets - Nivel 4 (Asignaciones)
        public DbSet<ActivityAssignment> ActivityAssignments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<AccessAudit> AccessAudits { get; set; }

        // DbSets - Nivel 5 (Respuestas)
        public DbSet<ActivityResponse> ActivityResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var currentUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = currentUserId ?? Guid.Empty;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = currentUserId;
                        entry.Property(nameof(AuditableBaseEntity.CreatedAt)).IsModified = false;
                        entry.Property(nameof(AuditableBaseEntity.CreatedBy)).IsModified = false;
                        break;
                }
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor?.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
