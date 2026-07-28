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
    public class DeactivateFamilyCommandHandlerTests
    {
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IIdentityService  _identity   = Substitute.For<IIdentityService>();
        private readonly IUnitOfWork       _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime   = Substitute.For<IDateTimeProvider>();

        private DeactivateFamilyCommandHandler BuildSut() =>
            new(_familyRepo, _identity, _uow,
                NullLogger<DeactivateFamilyCommandHandler>.Instance, _dateTime);

        private static readonly Guid FamilyId = Guid.NewGuid();

        private static FamilyRepresentative AFamily() => new()
        {
            UserId    = Guid.NewGuid(),
            FirstName = "María",
            LastName  = "López",
            IsActive  = true,
            User      = new User { IsActive = true },
        };

        // ── Familiar no encontrado ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_FamilyNotFound_ReturnsNotFound()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>())
                       .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(new DeactivateFamilyCommand(FamilyId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActiveFamily_DeactivatesUserAndFamilyAndSaves()
        {
            var family = AFamily();
            var now    = DateTime.UtcNow;
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _familyRepo.GetDependentStudentsWithNoOtherActiveRepresentativeCountAsync(family.Id, Arg.Any<CancellationToken>()).Returns(0);
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(new DeactivateFamilyCommand(FamilyId), default);

            result.Success.Should().BeTrue();
            family.IsActive.Should().BeFalse();
            family.User.IsActive.Should().BeFalse();
            family.UpdatedAt.Should().Be(now);
            await _familyRepo.Received(1).UpdateAsync(family, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_ActiveFamilyWithDependentStudents_ReturnsError()
        {
            var family = AFamily();
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _familyRepo.GetDependentStudentsWithNoOtherActiveRepresentativeCountAsync(family.Id, Arg.Any<CancellationToken>()).Returns(2);

            var result = await BuildSut().HandleAsync(new DeactivateFamilyCommand(FamilyId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
            result.Message.Should().Contain("No se puede desactivar al familiar porque es el único representante activo para 2 alumno(s)");
            family.IsActive.Should().BeTrue();
            family.User.IsActive.Should().BeTrue();
            await _familyRepo.DidNotReceive().UpdateAsync(Arg.Any<FamilyRepresentative>(), Arg.Any<CancellationToken>());
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
