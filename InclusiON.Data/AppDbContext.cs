using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InclusiON.Data.Converters;
using InclusiON.Domain.Attributes;
using InclusiON.Domain.Models;
using InclusiON.Domain.Models.BaseEntities;

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

        // Nivel 1: Catalogos base del sistema (tipos, categorias, metodos)
        public DbSet<DisabilityType> DisabilityTypes { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<EducationalInstitution> EducationalInstitutions { get; set; }
        public DbSet<AutonomyLevel> AutonomyLevels { get; set; }
        public DbSet<LoginMethod> LoginMethods { get; set; }
        public DbSet<SkillArea> SkillAreas { get; set; }
        public DbSet<ActivityTemplateType> ActivityTemplateTypes { get; set; }
        public DbSet<BackgroundJobStatus> BackgroundJobStatuses { get; set; }
        public DbSet<JobType> JobTypes { get; set; }

        // Nivel 2: Perfiles de usuario y autenticacion
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Professional> Professionals { get; set; }
        public DbSet<PersonWithDisability> PersonsWithDisability { get; set; }
        public DbSet<FamilyRepresentative> FamilyRepresentatives { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        // Nivel 3: Relaciones entre perfiles, actividades y rutas de aprendizaje
        public DbSet<AdminInstitution> AdminInstitutions { get; set; }
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<ProfessionalInstitution> ProfessionalInstitutions { get; set; }
        public DbSet<ProfessionalPerson> ProfessionalPersons { get; set; }
        public DbSet<PersonRepresentative> PersonRepresentatives { get; set; }
        public DbSet<PersonSkillProfile> PersonSkillProfiles { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityContent> ActivityContents { get; set; }
        public DbSet<PersonRoadmap> PersonRoadmaps { get; set; }
        public DbSet<PersonRoadmapArea> PersonRoadmapAreas { get; set; }
        public DbSet<PersonRoadmapActivity> PersonRoadmapActivities { get; set; }

        // Nivel 4: Asignaciones, reportes, mensajeria y auditoria
        public DbSet<ActivityAssignment> ActivityAssignments { get; set; }
        public DbSet<ActivityAssignmentStatus> ActivityAssignmentStatuses { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<AccessAudit> AccessAudits { get; set; }
        public DbSet<ProfessionalStatusHistory> ProfessionalStatusHistories { get; set; }
        public DbSet<FamilyStatusHistory> FamilyStatusHistories { get; set; }
        public DbSet<PersonRepresentativeHistory> PersonRepresentativeHistories { get; set; }

        // Nivel 5: Respuestas, resultados y embeddings de actividades
        public DbSet<ActivityResponse> ActivityResponses { get; set; }
        public DbSet<ActivityResult> ActivityResults { get; set; }
        public DbSet<ActivityEmbedding> ActivityEmbeddings { get; set; }

        // Nivel 6: Motor de dificultad adaptativa
        public DbSet<AdaptiveEngineConfig> AdaptiveEngineConfigs { get; set; }
        public DbSet<AdaptiveAdjustmentLog> AdaptiveAdjustmentLogs { get; set; }

        // Nivel 7: Background Jobs (procesamiento asincrono por agentes MAF)
        public DbSet<BackgroundJob> BackgroundJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            ApplyEncryptedConverters(builder);
        }

        private static void ApplyEncryptedConverters(ModelBuilder builder)
        {
            var converter = new EncryptedStringConverter();

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.ClrType.GetProperties()
                    .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null
                             && p.PropertyType == typeof(string)))
                {
                    builder.Entity(entityType.ClrType)
                           .Property(property.Name)
                           .HasConversion(converter);
                }
            }
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
