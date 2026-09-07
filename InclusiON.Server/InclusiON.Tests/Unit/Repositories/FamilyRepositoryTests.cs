using FluentAssertions;
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
    }
}
