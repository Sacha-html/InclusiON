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
    public class AcceptInvitationCommandHandlerTests
    {
        private readonly IInvitationsRepository _invRepo  = Substitute.For<IInvitationsRepository>();
        private readonly IIdentityService       _identity = Substitute.For<IIdentityService>();
        private readonly IUnitOfWork            _uow      = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider      _dateTime = Substitute.For<IDateTimeProvider>();

        private AcceptInvitationCommandHandler BuildSut() =>
            new(_invRepo, _identity, _uow,
                NullLogger<AcceptInvitationCommandHandler>.Instance, _dateTime);

        private static AcceptInvitationCommand Cmd(
            string code            = "abc-code",
            string email           = "nuevo@test.com",
            string password        = "Passw0rd!",
            string confirmPassword = "Passw0rd!") =>
            new(code, email, password, confirmPassword);

        private Invitation ValidInvitation() => new()
        {
            Code      = "abc-code",
            Email     = "invitado@test.com",
            IsUsed    = false,
            ExpiresAt = _dateTime.UtcNow.AddDays(7),
            FirstName = "Ana",
            LastName  = "Lopez",
        };

        private void SetupTransaction() =>
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));

        // ── Invitación no encontrada ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InvitationNotFound_ReturnsInvitationNotFound()
        {
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>())
                    .Returns((Invitation?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationNotFound);
        }

        // ── Invitación ya usada ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InvitationAlreadyUsed_ReturnsInvitationAlreadyUsed()
        {
            _dateTime.UtcNow.Returns(DateTime.UtcNow);
            var inv = ValidInvitation();
            inv.IsUsed = true;
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationAlreadyUsed);
        }

        // ── Invitación expirada ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InvitationExpired_ReturnsInvitationExpired()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(-1);
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvitationExpired);
        }

        // ── Contraseñas no coinciden ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PasswordMismatch_ReturnsValidationFailed()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(7);
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>()).Returns(inv);

            var result = await BuildSut().HandleAsync(Cmd(confirmPassword: "Diferente1!"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Email ya registrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailAlreadyExists_ReturnsEmailAlreadyExists()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(7);
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>()).Returns(inv);
            _identity.FindByEmailAsync("nuevo@test.com").Returns(new User());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidInvitation_CreatesUserAndMarkUsed()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            var inv = ValidInvitation();
            inv.ExpiresAt = now.AddDays(7);
            _invRepo.GetByCodeAsync("abc-code", Arg.Any<CancellationToken>()).Returns(inv);
            _identity.FindByEmailAsync("nuevo@test.com").Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Array.Empty<string>()));
            _invRepo.CreateFamilyRepresentativeAsync(Arg.Any<FamilyRepresentative>(), Arg.Any<CancellationToken>())
                    .Returns(ci => (FamilyRepresentative)ci[0]);
            SetupTransaction();

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            inv.IsUsed.Should().BeTrue();
            inv.UsedAt.Should().Be(now);
            await _identity.Received(1).CreateUserAsync(Arg.Any<User>(), "Passw0rd!");
            await _identity.Received(1).AddToRoleAsync(Arg.Any<User>(), "FamilyRepresentative");
        }
    }
}
