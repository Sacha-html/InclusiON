using FluentAssertions;
using Xunit;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Repositories
{
    public class ReportsRepositoryTests : DbContextTestBase
    {
        public ReportsRepositoryTests()
        {
            InclusiON.Data.Converters.EncryptionAccessor.Initialize(v => v, v => v);
        }
        [Fact]
        public async Task ReassignReportAsync_WhenProfessionalPersonIsAlreadyTrackedLocally_DoesNotThrowTrackingException()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var newProfUserId = Guid.NewGuid();
            var newProfId = Guid.NewGuid();

            var newProf = new Professional
            {
                Id = newProfId,
                UserId = newProfUserId,
                FirstName = "Nuevo",
                LastName = "Prof",
                Status = ProfessionalStatusEnum.Approved,
                IsActive = true,
                User = new User { Id = newProfUserId, IsActive = true }
            };
            Db.Professionals.Add(newProf);

            // Simular que el nuevo vínculo ProfessionalPerson ya fue agregado/trackeado en memoria (ej. por TransferStudentCommandHandler)
            var trackedLink = new ProfessionalPerson
            {
                ProfessionalId = newProfId,
                PersonId = personId,
                IsPrimaryProfessional = true,
                IsActive = true,
                CanSuperviseLogin = true,
                AssignedAt = DateTime.UtcNow
            };
            Db.ProfessionalPersons.Add(trackedLink);

            var report = new Report
            {
                Id = 100,
                PersonId = personId,
                ProfessionalId = Guid.NewGuid(), // profesional anterior
                Status = ReportStatus.Draft,
                IsActive = true,
                Title = "Informe inicial"
            };
            Db.Reports.Add(report);
            await Db.SaveChangesAsync();

            var repository = new ReportsRepository(Db);

            // Act: reasignar reporte debe usar la instancia local sin provocar InvalidOperationException
            Func<Task> act = () => repository.ReassignReportAsync(report, newProfId, DateTime.UtcNow);

            // Assert
            await act.Should().NotThrowAsync();
            report.ProfessionalId.Should().Be(newProfId);
        }
    }
}
