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
    public class ReactivatePersonCommandHandlerTests
    {
        private readonly IPersonsRepository _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork        _uow         = Substitute.For<IUnitOfWork>();

        private ReactivatePersonCommandHandler BuildSut() =>
            new(_personsRepo, _uow,
                NullLogger<ReactivatePersonCommandHandler>.Instance);

        private static readonly Guid PersonId = Guid.NewGuid();

        private static PersonWithDisability AnInactivePerson() => new()
        {
            Id     = PersonId,
            UserId = Guid.NewGuid(),
            User   = new User { IsActive = false },
        };

        // ── Persona no encontrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(new ReactivatePersonCommand(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Persona ya activa ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyActivePerson_ReturnsBusinessRuleViolation()
        {
            var person = new PersonWithDisability
            {
                Id = PersonId,
                UserId = Guid.NewGuid(),
                User = new User { IsActive = true }
            };
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);

            var result = await BuildSut().HandleAsync(new ReactivatePersonCommand(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InactivePerson_ReactivatesSuccessfully()
        {
            var person = AnInactivePerson();
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);

            var result = await BuildSut().HandleAsync(new ReactivatePersonCommand(PersonId), default);

            result.Success.Should().BeTrue();
            person.User.IsActive.Should().BeTrue();
            await _personsRepo.Received(1).UpdateAsync(person, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
