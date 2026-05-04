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
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(new DeactivateFamilyCommand(FamilyId), default);

            result.Success.Should().BeTrue();
            family.IsActive.Should().BeFalse();
            family.User.IsActive.Should().BeFalse();
            family.UpdatedAt.Should().Be(now);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
