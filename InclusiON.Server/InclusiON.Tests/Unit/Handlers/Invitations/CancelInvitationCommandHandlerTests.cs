using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Invitations
{
    public class CancelInvitationCommandHandlerTests
    {
        private readonly IInvitationsRepository _invRepo = Substitute.For<IInvitationsRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private CancelInvitationCommandHandler BuildSut() =>
            new(_invRepo, _uow, NullLogger<CancelInvitationCommandHandler>.Instance, _dateTime);

        [Fact]
        public async Task HandleAsync_InvitationNotFound_ReturnsInvitationNotFound()
        {
            _invRepo.GetByCodeAsync("unknown-code", Arg.Any<CancellationToken>())
                .Returns((Invitation?)null);

            var cmd = new CancelInvitationCommand("unknown-code", Guid.NewGuid(), "Professional", Guid.NewGuid());
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationNotFound);
        }

        [Fact]
        public async Task HandleAsync_ProfessionalNotOwner_ReturnsForbidden()
        {
            var ownerId = Guid.NewGuid();
            var otherProfId = Guid.NewGuid();
            var invitation = new Invitation
            {
                Code = "code-123",
                CreatedByProfessionalId = ownerId,
                IsActive = true,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            _invRepo.GetByCodeAsync("code-123", Arg.Any<CancellationToken>())
                .Returns(invitation);

            var cmd = new CancelInvitationCommand("code-123", Guid.NewGuid(), "Professional", otherProfId);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_InvitationAlreadyUsed_ReturnsInvalidOperation()
        {
            var profId = Guid.NewGuid();
            var invitation = new Invitation
            {
                Code = "code-123",
                CreatedByProfessionalId = profId,
                IsActive = true,
                IsUsed = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            _invRepo.GetByCodeAsync("code-123", Arg.Any<CancellationToken>())
                .Returns(invitation);

            var cmd = new CancelInvitationCommand("code-123", Guid.NewGuid(), "Professional", profId);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        [Fact]
        public async Task HandleAsync_InvitationAlreadyCancelled_ReturnsInvalidOperation()
        {
            var profId = Guid.NewGuid();
            var invitation = new Invitation
            {
                Code = "code-123",
                CreatedByProfessionalId = profId,
                IsActive = false,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            _invRepo.GetByCodeAsync("code-123", Arg.Any<CancellationToken>())
                .Returns(invitation);

            var cmd = new CancelInvitationCommand("code-123", Guid.NewGuid(), "Professional", profId);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        [Fact]
        public async Task HandleAsync_ValidProfessionalOwner_CancelsSuccessfully()
        {
            var profId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var fakeNow = new DateTime(2026, 10, 13, 15, 0, 0, DateTimeKind.Utc);
            _dateTime.UtcNow.Returns(fakeNow);

            var invitation = new Invitation
            {
                Id = 42,
                Code = "code-valid",
                Email = "familiar@test.com",
                CreatedByProfessionalId = profId,
                IsActive = true,
                IsUsed = false,
                ExpiresAt = fakeNow.AddDays(5)
            };

            _invRepo.GetByCodeAsync("code-valid", Arg.Any<CancellationToken>())
                .Returns(invitation);

            var cmd = new CancelInvitationCommand("code-valid", userId, "Professional", profId);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Status.Should().Be("Cancelada");
            invitation.IsActive.Should().BeFalse();
            invitation.UpdatedBy.Should().Be(userId);
            invitation.UpdatedAt.Should().Be(fakeNow);

            await _invRepo.Received(1).UpdateAsync(invitation, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AdminCancelsInvitation_SucceedsEvenIfNotOwner()
        {
            var profId = Guid.NewGuid();
            var adminUserId = Guid.NewGuid();
            var fakeNow = new DateTime(2026, 10, 13, 15, 0, 0, DateTimeKind.Utc);
            _dateTime.UtcNow.Returns(fakeNow);

            var invitation = new Invitation
            {
                Id = 42,
                Code = "code-admin",
                Email = "familiar@test.com",
                CreatedByProfessionalId = profId,
                IsActive = true,
                IsUsed = false,
                ExpiresAt = fakeNow.AddDays(5)
            };

            _invRepo.GetByCodeAsync("code-admin", Arg.Any<CancellationToken>())
                .Returns(invitation);

            var cmd = new CancelInvitationCommand("code-admin", adminUserId, "Administrator", null);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            result.Data!.Status.Should().Be("Cancelada");
            invitation.IsActive.Should().BeFalse();
            invitation.UpdatedBy.Should().Be(adminUserId);
            invitation.UpdatedAt.Should().Be(fakeNow);
        }
    }
}
