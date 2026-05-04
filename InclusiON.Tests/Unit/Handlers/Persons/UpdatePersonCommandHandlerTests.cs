using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class UpdatePersonCommandHandlerTests
    {
        private readonly IPersonsRepository _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork        _uow         = Substitute.For<IUnitOfWork>();

        private UpdatePersonCommandHandler BuildSut() =>
            new(_personsRepo, _uow, NullLogger<UpdatePersonCommandHandler>.Instance);

        private static readonly Guid PersonId = Guid.NewGuid();

        private static UpdatePersonCommand Cmd(string? doc = null) =>
            new(PersonId, "Nuevo", "Apellido", doc,
                null, null, null,
                null, null, null, null, null,
                null, null, null, null,
                null, null, null, null, null,
                null, null, null);

        private static PersonWithDisability APerson() => new()
        {
            Id             = PersonId,
            UserId         = Guid.NewGuid(),
            FirstName      = "Viejo",
            DocumentNumber = "11111111",
            User           = new User { IsActive = true },
        };

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

        // ── Documento duplicado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DuplicateDocument_ReturnsDocumentAlreadyExists()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
            _personsRepo.ExistsDocumentAsync("99999999", PersonId, Arg.Any<CancellationToken>())
                        .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(doc: "99999999"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_UpdatesAndSaves()
        {
            var person = APerson();
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            person.FirstName.Should().Be("Nuevo");
            await _personsRepo.Received(1).UpdateAsync(person, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
