using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class GetAdminUserDetailQueryHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IProfessionalsRepository _proRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository _personRepo = Substitute.For<IPersonsRepository>();
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IAdminInstitutionRepository _adminInstRepo = Substitute.For<IAdminInstitutionRepository>();
        private readonly IAssignmentsRepository _assignmentsRepo = Substitute.For<IAssignmentsRepository>();

        private static readonly Guid UserId = Guid.NewGuid();

        private GetAdminUserDetailQueryHandler BuildSut() =>
            new(_identity, _proRepo, _personRepo, _familyRepo, _adminInstRepo, _assignmentsRepo);

        private static User AUser() => new()
        {
            Id = UserId,
            Email = "user@test.com",
            Name = "Juan",
            Surname = "Pérez",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        [Fact]
        public async Task UserNotFound_ReturnsNotFound()
        {
            _identity.FindByIdAsync(UserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(new GetAdminUserDetailQuery(UserId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task UserFound_WithProfessionalLinked_ReturnsProfessionalEntity()
        {
            var user = AUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Professional" });

            var pro = new Professional
            {
                Id = Guid.NewGuid(), UserId = UserId,
                FirstName = "Carlos", LastName = "López",
                Specialty = "Psicología", LicenseNumber = "PSY-001"
            };
            _proRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(pro);

            var result = await BuildSut().HandleAsync(new GetAdminUserDetailQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data!.FullName.Should().Be("Carlos López");
            result.Data.LinkedEntity!.EntityType.Should().Be("Professional");
            result.Data.LinkedEntity.Specialty.Should().Be("Psicología");
        }

        [Fact]
        public async Task UserFound_WithPersonLinked_ReturnsPersonEntity()
        {
            var user = AUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Person" });

            _proRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Professional?)null);

            var person = new PersonWithDisability
            {
                Id = Guid.NewGuid(), UserId = UserId,
                FirstName = "Ana", LastName = "García",
                BirthDate = new DateTime(2000, 1, 1)
            };
            _personRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);

            var result = await BuildSut().HandleAsync(new GetAdminUserDetailQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data!.FullName.Should().Be("Ana García");
            result.Data.LinkedEntity!.EntityType.Should().Be("PersonWithDisability");
        }

        [Fact]
        public async Task UserFound_WithFamilyLinked_ReturnsFamilyEntity()
        {
            var user = AUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Family" });

            _proRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Professional?)null);
            _personRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((PersonWithDisability?)null);

            var family = new FamilyRepresentative
            {
                Id = Guid.NewGuid(), UserId = UserId,
                FirstName = "María", LastName = "Ruiz",
                Relationship = "Madre", Phone = "123456789"
            };
            _familyRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(family);

            var result = await BuildSut().HandleAsync(new GetAdminUserDetailQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data!.FullName.Should().Be("María Ruiz");
            result.Data.LinkedEntity!.EntityType.Should().Be("FamilyRepresentative");
            result.Data.LinkedEntity.Relationship.Should().Be("Madre");
        }

        [Fact]
        public async Task UserFound_NoLinkedEntity_ReturnsUserNameAsFullName()
        {
            var user = AUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "GlobalAdmin" });

            _proRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Professional?)null);
            _personRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((PersonWithDisability?)null);
            _familyRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(new GetAdminUserDetailQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data!.FullName.Should().Be("Juan Pérez");
            result.Data.LinkedEntity.Should().BeNull();
            result.Data.Role.Should().Be("GlobalAdmin");
        }
    }
}
