using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class LinkFamilyToPersonCommandHandlerTests
    {
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IUnitOfWork       _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime   = Substitute.For<IDateTimeProvider>();

        private LinkFamilyToPersonCommandHandler BuildSut() =>
            new(_familyRepo, _uow,
                NullLogger<LinkFamilyToPersonCommandHandler>.Instance, _dateTime);

        private static readonly Guid FamilyId = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid AdminId  = Guid.NewGuid();

        private static LinkFamilyToPersonCommand Cmd(string rel = "Tutor") =>
            new(FamilyId, PersonId, rel, isPrimary: false, AdminId);

        private static FamilyRepresentative ActiveFamily() => new()
        {
            UserId = Guid.NewGuid(),
            Status = FamilyStatusEnum.Active,
            User   = new User { IsActive = true },
        };

        private void SetupTransaction() =>
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));

        // ── Familiar no encontrado ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_FamilyNotFound_ReturnsNotFound()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>())
                       .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Familiar inactivo ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_FamilyInactive_ReturnsValidationFailed()
        {
            var family = ActiveFamily();
            family.User.IsActive = false;
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Vínculo ya activo ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyLinked_ReturnsConflict()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(ActiveFamily());
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns(new PersonRepresentative { IsActive = true });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        // ── Relación única ya ocupada (Madre/Padre) ──────────────────────────

        [Fact]
        public async Task HandleAsync_DuplicateMadre_ReturnsConflict()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(ActiveFamily());
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns((PersonRepresentative?)null);
            _familyRepo.GetPersonRepresentativesByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                       .Returns(new List<PersonRepresentative>
                       {
                           new() { IsActive = true, Relationship = "Madre" }
                       });

            var result = await BuildSut().HandleAsync(Cmd("Madre"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        // ── Happy path (nuevo vínculo) ───────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidLink_CreatesPersonRepresentativeAndSaves()
        {
            var family = ActiveFamily();
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns((PersonRepresentative?)null);
            _familyRepo.GetPersonRepresentativesByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                       .Returns(new List<PersonRepresentative>());
            SetupTransaction();
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _familyRepo.Received(1).CreatePersonRepresentativeAsync(
                Arg.Is<PersonRepresentative>(pr => pr.PersonId == PersonId && pr.IsActive),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
