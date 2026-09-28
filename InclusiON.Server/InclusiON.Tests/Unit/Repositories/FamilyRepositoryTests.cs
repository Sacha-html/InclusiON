using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Repositories
{
    public class FamilyRepositoryTests : DbContextTestBase
    {
        [Fact]
        public async Task GetByIdForUpdateAsync_WhenDeactivationLoadsTheSameFamily_DoesNotTrackADuplicate()
        {
            var userId = Guid.NewGuid();
            var family = new FamilyRepresentative
            {
                UserId = userId,
                FirstName = "Maria",
                LastName = "Lopez",
                User = new User { Id = userId, IsActive = true },
            };
            Db.FamilyRepresentatives.Add(family);
            await Db.SaveChangesAsync();
            Db.ChangeTracker.Clear();

            var repository = new FamilyRepository(Db);
            var familyForUpdate = await repository.GetByIdForUpdateAsync(family.Id);

            await repository.DeactivateRepresentativeAndSuspendDependentStudentsAsync(userId, DateTime.UtcNow);
            Func<Task> act = () => repository.UpdateAsync(familyForUpdate!);

            await act.Should().NotThrowAsync();
            Db.ChangeTracker.Entries<FamilyRepresentative>().Should().ContainSingle();
        }

        [Fact]
        public async Task DeactivateThenRestore_RestoresLinkAndStudent_ButLeavesManualUnlinksAlone()
        {
            var familyUserId = Guid.NewGuid();
            var family = new FamilyRepresentative
            {
                UserId = familyUserId,
                FirstName = "Ana",
                LastName = "Rodriguez",
                User = new User { Id = familyUserId, IsActive = true },
            };

            var studentUserId = Guid.NewGuid();
            var student = new PersonWithDisability
            {
                FirstName = "Pedro",
                LastName = "Rodriguez",
                IsActive = true,
                User = new User { Id = studentUserId, IsActive = true },
            };

            // Un segundo alumno ya desvinculado manualmente (no por la baja de este familiar):
            // no debe verse afectado por la restauracion.
            var otherStudentUserId = Guid.NewGuid();
            var otherStudent = new PersonWithDisability
            {
                FirstName = "Sofia",
                LastName = "Gomez",
                IsActive = false,
                User = new User { Id = otherStudentUserId, IsActive = false },
            };

            Db.FamilyRepresentatives.Add(family);
            Db.PersonsWithDisability.AddRange(student, otherStudent);
            await Db.SaveChangesAsync();

            var link = new PersonRepresentative
            {
                PersonId = student.Id,
                RepresentativeId = family.Id,
                IsActive = true,
                IsPrimary = true,
            };
            var manuallyUnlinkedLink = new PersonRepresentative
            {
                PersonId = otherStudent.Id,
                RepresentativeId = family.Id,
                IsActive = false,
                UnlinkObservation = "Desvinculado a pedido de la familia",
            };
            Db.PersonRepresentatives.AddRange(link, manuallyUnlinkedLink);
            await Db.SaveChangesAsync();
            Db.ChangeTracker.Clear();

            var repository = new FamilyRepository(Db);

            var suspended = await repository.DeactivateRepresentativeAndSuspendDependentStudentsAsync(familyUserId, DateTime.UtcNow);
            await Db.SaveChangesAsync();
            Db.ChangeTracker.Clear();

            suspended.Should().ContainSingle().Which.Should().Be("Pedro Rodriguez");
            (await Db.PersonsWithDisability.FindAsync(student.Id))!.IsActive.Should().BeFalse();

            var restored = await repository.RestoreSystemSuspendedLinksAsync(family.Id, DateTime.UtcNow);
            await Db.SaveChangesAsync();
            Db.ChangeTracker.Clear();

            restored.Should().ContainSingle().Which.Should().Be("Pedro Rodriguez");

            var restoredStudent = await Db.PersonsWithDisability.Include(p => p.User).FirstAsync(p => p.Id == student.Id);
            restoredStudent.IsActive.Should().BeTrue();
            restoredStudent.User!.IsActive.Should().BeTrue();

            var restoredLink = await Db.PersonRepresentatives.FirstAsync(pr => pr.Id == link.Id);
            restoredLink.IsActive.Should().BeTrue();
            restoredLink.UnlinkObservation.Should().BeNull();

            // El vinculo desvinculado manualmente sigue intacto: no lo toca la restauracion.
            var untouchedLink = await Db.PersonRepresentatives.FirstAsync(pr => pr.Id == manuallyUnlinkedLink.Id);
            untouchedLink.IsActive.Should().BeFalse();
            untouchedLink.UnlinkObservation.Should().Be("Desvinculado a pedido de la familia");

            var untouchedStudent = await Db.PersonsWithDisability.FindAsync(otherStudent.Id);
            untouchedStudent!.IsActive.Should().BeFalse();
        }
    }
}
