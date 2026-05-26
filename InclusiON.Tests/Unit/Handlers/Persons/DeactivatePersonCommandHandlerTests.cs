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
    public class DeactivatePersonCommandHandlerTests
    {
        private readonly IPersonsRepository       _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IRefreshTokensRepository _tokenRepo   = Substitute.For<IRefreshTokensRepository>();
        private readonly IUnitOfWork              _uow         = Substitute.For<IUnitOfWork>();

        private DeactivatePersonCommandHandler BuildSut() =>
            new(_personsRepo, _tokenRepo, _uow,
                NullLogger<DeactivatePersonCommandHandler>.Instance);

        private static readonly Guid PersonId = Guid.NewGuid();

        private static PersonWithDisability APerson() => new()
        {
            Id     = PersonId,
            UserId = Guid.NewGuid(),
            User   = new User { IsActive = true },
        };

        // ── Persona no encontrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(new DeactivatePersonCommand(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActivePerson_DeactivatesAndRevokesTokens()
        {
            var person = APerson();
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);

            var result = await BuildSut().HandleAsync(new DeactivatePersonCommand(PersonId), default);

            result.Success.Should().BeTrue();
            person.User.IsActive.Should().BeFalse();
            await _tokenRepo.Received(1).RevokeAllUserTokensAsync(
                person.UserId, Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
