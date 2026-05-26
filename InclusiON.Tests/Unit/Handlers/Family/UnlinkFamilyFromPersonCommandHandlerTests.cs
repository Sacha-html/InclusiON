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
    public class UnlinkFamilyFromPersonCommandHandlerTests
    {
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IUnitOfWork       _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime   = Substitute.For<IDateTimeProvider>();

        private UnlinkFamilyFromPersonCommandHandler BuildSut() =>
            new(_familyRepo, _uow,
                NullLogger<UnlinkFamilyFromPersonCommandHandler>.Instance, _dateTime);

        private static readonly Guid FamilyId = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid AdminId  = Guid.NewGuid();

        private static UnlinkFamilyFromPersonCommand Cmd(string obs = "Cambio de tutor") =>
            new(FamilyId, PersonId, obs, AdminId);

        private static PersonRepresentative ActiveLink() =>
            new() { IsActive = true, Relationship = "Tutor", IsPrimary = false };

        private static FamilyRepresentative AFamily() => new()
        {
            UserId    = Guid.NewGuid(),
            FirstName = "María",
            LastName  = "López",
            User      = new User { IsActive = true },
        };

        private void SetupTransaction() =>
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));

        // ── Vínculo no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_LinkNotFound_ReturnsNotFound()
        {
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns((PersonRepresentative?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Vínculo ya inactivo ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_LinkAlreadyInactive_ReturnsValidationFailed()
        {
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns(new PersonRepresentative { IsActive = false });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Sin motivo de desvinculación ─────────────────────────────────────

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task HandleAsync_EmptyObservation_ReturnsValidationFailed(string obs)
        {
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns(ActiveLink());
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(AFamily());

            var result = await BuildSut().HandleAsync(Cmd(obs), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActiveLink_DeactivatesAndCreatesHistory()
        {
            var link   = ActiveLink();
            var family = AFamily();
            var now    = DateTime.UtcNow;
            _familyRepo.GetPersonRepresentativeAsync(PersonId, FamilyId, Arg.Any<CancellationToken>())
                       .Returns(link);
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            SetupTransaction();
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            link.IsActive.Should().BeFalse();
            link.EndedAt.Should().Be(now);
            link.UnlinkObservation.Should().Be("Cambio de tutor");
            await _familyRepo.Received(1).CreatePersonRepresentativeHistoryAsync(
                Arg.Any<PersonRepresentativeHistory>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
