using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Handlers;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Invitations
{
    public class ValidateInvitationQueryHandlerTests
    {
        private readonly IInvitationsRepository _repo     = Substitute.For<IInvitationsRepository>();
        private readonly IDateTimeProvider      _dateTime = Substitute.For<IDateTimeProvider>();

        private ValidateInvitationQueryHandler BuildSut() =>
            new(_repo, NullLogger<ValidateInvitationQueryHandler>.Instance, _dateTime);

        private Invitation ValidInvitation() => new()
        {
            Code = "code-xyz", Email = "x@y.com", IsUsed = false,
            ExpiresAt = _dateTime.UtcNow.AddDays(7),
            FirstName = "Ana",
        };

        // ── No encontrada ────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NotFound_ReturnsInvitationNotFound()
        {
            _repo.GetByCodeAsync("code-xyz", Arg.Any<CancellationToken>())
                 .Returns((Invitation?)null);

            var result = await BuildSut().HandleAsync(new ValidateInvitationQuery("code-xyz"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationNotFound);
        }

        // ── Ya usada ─────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyUsed_ReturnsInvitationAlreadyUsed()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.IsUsed = true;
            _repo.GetByCodeAsync("code-xyz", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(new ValidateInvitationQuery("code-xyz"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationAlreadyUsed);
        }

        // ── Expirada ─────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Expired_ReturnsInvitationExpired()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(-1);
            _repo.GetByCodeAsync("code-xyz", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(new ValidateInvitationQuery("code-xyz"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationExpired);
        }

        // ── Válida ───────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidInvitation_ReturnsCode()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(7);
            _repo.GetByCodeAsync("code-xyz", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(new ValidateInvitationQuery("code-xyz"), default);

            result.Success.Should().BeTrue();
            result.Data!.Code.Should().Be("code-xyz");
            result.Data.FirstName.Should().Be("Ana");
        }
    }
}
