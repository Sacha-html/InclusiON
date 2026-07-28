using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class CreateFamilyCommandHandlerTests
    {
        private readonly IFamilyRepository    _familyRepo  = Substitute.For<IFamilyRepository>();
        private readonly IPersonsRepository   _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IIdentityService     _identity    = Substitute.For<IIdentityService>();
        private readonly IBackgroundJobRepository _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork          _uow         = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider    _dateTime    = Substitute.For<IDateTimeProvider>();

        private CreateFamilyCommandHandler BuildSut() =>
            new(_familyRepo, _personsRepo, _identity, _backgroundJobs, _uow,
                NullLogger<CreateFamilyCommandHandler>.Instance, _dateTime);

        private static readonly Guid PersonId = Guid.NewGuid();

        private static CreateFamilyCommand Cmd(string? doc = null) =>
            new("María", "López", "maria@test.com", doc, null, "Madre", PersonId);

        private void SetupTransaction()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _familyRepo.CreateAsync(Arg.Any<FamilyRepresentative>(), Arg.Any<CancellationToken>())
                       .Returns(ci => (FamilyRepresentative)ci[0]);
        }

        // ── Documento duplicado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DocumentExists_ReturnsDocumentAlreadyExists()
        {
            _familyRepo.ExistsDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                       .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Persona no encontrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Email ya registrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailExists_ReturnsEmailAlreadyExists()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability { Id = PersonId });
            _identity.FindByEmailAsync("maria@test.com")
                     .Returns(new User { Id = Guid.NewGuid() });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Fallo de Identity al crear usuario ───────────────────────────────

        [Fact]
        public async Task HandleAsync_IdentityCreateFails_ReturnsValidationFailed()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability { Id = PersonId });
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((false, new[] { "Password too weak" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesFamilyAndReturnsTemporaryPassword()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability { Id = PersonId });
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            SetupTransaction();
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.TemporaryPassword.Should().NotBeNullOrEmpty();
            await _identity.Received(1).CreateUserAsync(
                Arg.Is<User>(u => u.Email == "maria@test.com" && u.MustChangePassword),
                Arg.Any<string>());
        }
    }
}
