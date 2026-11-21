using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Services;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Services
{
    public class UniquenessValidationServiceTests : DbContextTestBase
    {
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private UniquenessValidationService BuildSut()
        {
            _dateTime.UtcNow.Returns(new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc));
            return new UniquenessValidationService(Db, _dateTime);
        }

        [Fact]
        public async Task CheckDocumentAsync_DuplicateAcrossRoles_DetectsConflictFromPersonFamilyAndProfessional()
        {
            var personUser = new User { Id = Guid.NewGuid(), Email = "a@test.local", NormalizedEmail = "A@TEST.LOCAL", IsActive = true };
            var familyUser = new User { Id = Guid.NewGuid(), Email = "f@test.com", NormalizedEmail = "F@TEST.COM", IsActive = true };
            var proUser = new User { Id = Guid.NewGuid(), Email = "p@test.com", NormalizedEmail = "P@TEST.COM", IsActive = true };

            var person = new PersonWithDisability { Id = Guid.NewGuid(), UserId = personUser.Id, User = personUser, FirstName = "A", LastName = "A", DocumentNumber = "11111111", IsActive = true };
            var family = new FamilyRepresentative { Id = Guid.NewGuid(), UserId = familyUser.Id, User = familyUser, FirstName = "F", LastName = "F", DocumentNumber = "22222222", IsActive = true };
            var pro = new Professional { Id = Guid.NewGuid(), UserId = proUser.Id, User = proUser, FirstName = "P", LastName = "P", DocumentNumber = "33333333", LicenseNumber = "MP-100", Email = "p@test.com", IsActive = true };

            Db.Users.AddRange(personUser, familyUser, proUser);
            Db.PersonsWithDisability.Add(person);
            Db.FamilyRepresentatives.Add(family);
            Db.Professionals.Add(pro);
            await Db.SaveChangesAsync();

            var sut = BuildSut();

            // Alumno activo
            var r1 = await sut.CheckDocumentAsync(" 11111111 ");
            r1.IsAvailable.Should().BeFalse();
            r1.ConflictRole.Should().Be("Alumno");
            r1.IsInactiveStudent.Should().BeFalse();

            // Familiar
            var r2 = await sut.CheckDocumentAsync("22222222");
            r2.IsAvailable.Should().BeFalse();
            r2.ConflictRole.Should().Be("Familiar");

            // Profesional
            var r3 = await sut.CheckDocumentAsync("33333333");
            r3.IsAvailable.Should().BeFalse();
            r3.ConflictRole.Should().Be("Profesional");

            // Exclusión del propio ID en edición
            var rSelf = await sut.CheckDocumentAsync("22222222", excludeEntityId: family.Id);
            rSelf.IsAvailable.Should().BeTrue();

            // DNI disponible
            var rFree = await sut.CheckDocumentAsync("44444444");
            rFree.IsAvailable.Should().BeTrue();
        }

        [Fact]
        public async Task CheckDocumentAsync_InactiveStudent_RejectsAndFlagsIsInactiveStudent()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "inactivo@test.local", NormalizedEmail = "INACTIVO@TEST.LOCAL", IsActive = false };
            var person = new PersonWithDisability { Id = Guid.NewGuid(), UserId = user.Id, User = user, FirstName = "Luis", LastName = "Gomez", DocumentNumber = "55555555", IsActive = false };
            Db.Users.Add(user);
            Db.PersonsWithDisability.Add(person);
            await Db.SaveChangesAsync();

            var result = await BuildSut().CheckDocumentAsync("55555555");

            result.IsAvailable.Should().BeFalse();
            result.IsInactiveStudent.Should().BeTrue();
            result.Message.Should().Contain("reactivar");
        }

        [Fact]
        public async Task CheckEmailAsync_CaseInsensitiveAndTrimmed_DetectsUsersProfessionalsAndPendingInvitations()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "Admin@Escuela.Edu.Ar", NormalizedEmail = "ADMIN@ESCUELA.EDU.AR", IsActive = true };
            var familyUser = new User { Id = Guid.NewGuid(), Email = "mama@test.com", NormalizedEmail = "MAMA@TEST.COM", IsActive = true };
            var family = new FamilyRepresentative { Id = Guid.NewGuid(), UserId = familyUser.Id, User = familyUser, FirstName = "Ana", LastName = "Perez" };
            var pendingInv = new Invitation
            {
                Id = 1,
                Code = "code-1",
                Email = "invitado@test.com",
                IsActive = true,
                IsUsed = false,
                ExpiresAt = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc)
            };

            Db.Users.AddRange(user, familyUser);
            Db.FamilyRepresentatives.Add(family);
            Db.Invitations.Add(pendingInv);
            await Db.SaveChangesAsync();

            var sut = BuildSut();

            // Case-insensitive + whitespace
            (await sut.CheckEmailAsync("  admin@escuela.edu.ar  ")).IsAvailable.Should().BeFalse();

            // Excluir propio UserId (edición de admin)
            (await sut.CheckEmailAsync("admin@escuela.edu.ar", excludeUserId: user.Id)).IsAvailable.Should().BeTrue();

            // Excluir por EntityId (edición de familiar sin pasar UserId)
            (await sut.CheckEmailAsync("MAMA@TEST.COM", excludeEntityId: family.Id)).IsAvailable.Should().BeTrue();

            // Invitación pendiente activa: solo bloquea cuando checkPendingInvitations = true
            (await sut.CheckEmailAsync("INVITADO@TEST.COM", checkPendingInvitations: false)).IsAvailable.Should().BeTrue();
            (await sut.CheckEmailAsync("INVITADO@TEST.COM", checkPendingInvitations: true)).IsAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task CheckLicenseNumberAsync_CaseInsensitiveAndExcludesSelf()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "doc@test.com", NormalizedEmail = "DOC@TEST.COM", IsActive = true };
            var pro = new Professional
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                FirstName = "Laura",
                LastName = "Diaz",
                LicenseNumber = "MN-12345",
                Status = ProfessionalStatusEnum.Approved
            };

            Db.Users.Add(user);
            Db.Professionals.Add(pro);
            await Db.SaveChangesAsync();

            var sut = BuildSut();

            (await sut.CheckLicenseNumberAsync("  mn-12345  ")).IsAvailable.Should().BeFalse();
            (await sut.CheckLicenseNumberAsync("MN-12345", excludeProfessionalId: pro.Id)).IsAvailable.Should().BeTrue();
            (await sut.CheckLicenseNumberAsync("MN-99999")).IsAvailable.Should().BeTrue();
        }
    }
}
