using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InclusiON.Tests.Integration.TestSupport
{
    /// <summary>
    /// Fixture de integración para tests de autorización por recurso (HU-IN-172).
    /// Extiende <see cref="IntegrationTestFactory"/> y siembra una vez todos los datos
    /// necesarios para la matriz rol × entidad × acción (CA-14 / CA-15).
    ///
    /// xUnit crea la fixture una sola vez por clase de test (IClassFixture) y llama
    /// a InitializeAsync() antes del primer test, garantizando aislamiento total.
    /// </summary>
    public class AuthorizationTestFixture : IntegrationTestFactory, IAsyncLifetime
    {
        // ── IDs del JWT (userId en los tokens) ──────────────────────────────
        public Guid AssignedProfessionalUserId   { get; } = Guid.NewGuid();
        public Guid UnassignedProfessionalUserId { get; } = Guid.NewGuid();
        public Guid FamilyWithLinkUserId         { get; } = Guid.NewGuid();
        public Guid FamilyWithoutLinkUserId      { get; } = Guid.NewGuid();
        public Guid GlobalAdminUserId            { get; } = Guid.NewGuid();

        // ── IDs de entidades sensibles ──────────────────────────────────────
        public Guid PersonId    { get; } = Guid.NewGuid();
        public int  DiagnosisId { get; private set; }
        public int  ReportId    { get; private set; }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ── Users ────────────────────────────────────────────────────────
            var personUserId = Guid.NewGuid();
            db.Users.AddRange(
                BuildUser(personUserId,                    "person@test.com"),
                BuildUser(AssignedProfessionalUserId,      "profasign@test.com"),
                BuildUser(UnassignedProfessionalUserId,    "profnone@test.com"),
                BuildUser(FamilyWithLinkUserId,            "familylink@test.com"),
                BuildUser(FamilyWithoutLinkUserId,         "familynone@test.com"),
                BuildUser(GlobalAdminUserId,               "admin@test.com"));

            // ── Persona con discapacidad ──────────────────────────────────────
            db.PersonsWithDisability.Add(new PersonWithDisability
            {
                Id        = PersonId,
                UserId    = personUserId,
                FirstName = "Test",
                LastName  = "Person",
                BirthDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ── Profesional asignado ──────────────────────────────────────────
            var assignedProfId = Guid.NewGuid();
            db.Professionals.Add(new Professional
            {
                Id        = assignedProfId,
                UserId    = AssignedProfessionalUserId,
                FirstName = "Prof",
                LastName  = "Assigned"
            });
            db.ProfessionalPersons.Add(new ProfessionalPerson
            {
                ProfessionalId = assignedProfId,
                PersonId       = PersonId,
                IsActive       = true
            });

            // ── Profesional NO asignado ───────────────────────────────────────
            db.Professionals.Add(new Professional
            {
                Id        = Guid.NewGuid(),
                UserId    = UnassignedProfessionalUserId,
                FirstName = "Prof",
                LastName  = "Unassigned"
            });

            // ── Familiar con vínculo activo ───────────────────────────────────
            var familyWithLinkId = Guid.NewGuid();
            db.FamilyRepresentatives.Add(new FamilyRepresentative
            {
                Id        = familyWithLinkId,
                UserId    = FamilyWithLinkUserId,
                FirstName = "Mama",
                LastName  = "Link"
            });
            db.PersonRepresentatives.Add(new PersonRepresentative
            {
                Id               = Guid.NewGuid(),
                PersonId         = PersonId,
                RepresentativeId = familyWithLinkId,
                IsActive         = true
            });

            // ── Familiar sin vínculo ──────────────────────────────────────────
            db.FamilyRepresentatives.Add(new FamilyRepresentative
            {
                Id        = Guid.NewGuid(),
                UserId    = FamilyWithoutLinkUserId,
                FirstName = "Sin",
                LastName  = "Vinculo"
            });

            // ── Diagnóstico ──────────────────────────────────────────────────
            var diagnosis = new Diagnosis
            {
                Id               = 101,
                PersonId         = PersonId,
                ProfessionalId   = assignedProfId,
                DiagnosisDate    = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                PrimaryDiagnosis = "TEA leve"
            };
            db.Diagnoses.Add(diagnosis);

            // ── Reporte aprobado ─────────────────────────────────────────────
            var report = new Report
            {
                Id             = 201,
                PersonId       = PersonId,
                ProfessionalId = assignedProfId,
                ReportTypeId   = 1,
                Title          = "Reporte de seguimiento",
                Content        = "Contenido de prueba",
                ReportDate     = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Status         = ReportStatus.Approved
            };
            db.Reports.Add(report);

            await db.SaveChangesAsync();

            DiagnosisId = diagnosis.Id;
            ReportId    = report.Id;
        }

        public override async Task DisposeAsync() => await base.DisposeAsync();

        private static User BuildUser(Guid id, string email) => new()
        {
            Id             = id,
            UserName       = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email          = email,
            NormalizedEmail = email.ToUpperInvariant(),
            IsActive       = true,
            SecurityStamp  = Guid.NewGuid().ToString()
        };
    }
}
