using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class IdentifyUserQueryHandlerTests
    {
        private readonly IVisualLoginRepository _repo = Substitute.For<IVisualLoginRepository>();

        private IdentifyUserQueryHandler BuildSut() =>
            new(_repo, NullLogger<IdentifyUserQueryHandler>.Instance);

        private static IdentifyUserQuery Query(string identifier, string? userType = null, string? deviceId = null) =>
            new(identifier, deviceId, userType);

        // ── Too short ────────────────────────────────────────────────────

        [Fact]
        public async Task Identifier_TooShort_ReturnsUserFoundFalseWithMessage()
        {
            var result = await BuildSut().HandleAsync(Query("ab"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeFalse();
            result.Data.ErrorMessage.Should().Contain("3");
        }

        // ── UserType = PERSON ────────────────────────────────────────────

        [Fact]
        public async Task UserType_Person_NotFound_ReturnsUserFoundFalse()
        {
            _repo.FindPersonsByIdentifierAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<PersonWithDisability>());

            var result = await BuildSut().HandleAsync(Query("Ana", "PERSON"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeFalse();
        }

        [Fact]
        public async Task UserType_Person_SingleMatch_ReturnsUser()
        {
            var userId = Guid.NewGuid();
            var person = new PersonWithDisability
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "Ana",
                LastName = "García",
                LoginMethod = new LoginMethod { Code = "STANDARD", Name = "Contraseña", IsActive = true }
            };

            _repo.FindPersonsByIdentifierAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<PersonWithDisability> { person });
            _repo.IsTrustedDeviceAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await BuildSut().HandleAsync(Query("Ana", "PERSON"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeTrue();
            result.Data.UserId.Should().Be(userId);
            result.Data.UserType.Should().Be("Person");
            result.Data.LoginMethodCode.Should().Be("STANDARD");
        }

        // ── UserType = PROFESSIONAL ──────────────────────────────────────

        [Fact]
        public async Task UserType_Professional_Found_ReturnsUser()
        {
            var userId = Guid.NewGuid();
            var pro = new Professional
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "Carlos",
                LastName = "López"
            };

            _repo.FindProfessionalByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(pro);
            _repo.IsTrustedDeviceAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await BuildSut().HandleAsync(Query("carlos@mail.com", "PROFESSIONAL"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeTrue();
            result.Data.UserType.Should().Be("Professional");
            result.Data.LoginMethodCode.Should().Be("STANDARD");
        }

        // ── UserType = FAMILY ────────────────────────────────────────────

        [Fact]
        public async Task UserType_Family_Found_ReturnsUser()
        {
            var userId = Guid.NewGuid();
            var family = new FamilyRepresentative
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "María",
                LastName = "Ruiz"
            };

            _repo.FindFamilyByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(family);
            _repo.IsTrustedDeviceAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await BuildSut().HandleAsync(Query("maria@mail.com", "FAMILY"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeTrue();
            result.Data.UserType.Should().Be("Family");
        }

        // ── Default (no UserType) — fallback chain ───────────────────────

        [Fact]
        public async Task NoUserType_NoneFound_ReturnsUserFoundFalse()
        {
            _repo.FindPersonsByIdentifierAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<PersonWithDisability>());
            _repo.FindProfessionalByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Professional?)null);
            _repo.FindFamilyByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(Query("desconocido"), default);

            result.Success.Should().BeTrue();
            result.Data!.UserFound.Should().BeFalse();
        }
    }
}
