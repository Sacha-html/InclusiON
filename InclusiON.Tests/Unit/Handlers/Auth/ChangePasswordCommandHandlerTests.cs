using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

        private ChangePasswordCommandHandler BuildSut() =>
            new(_identity, NullLogger<ChangePasswordCommandHandler>.Instance);

        private static readonly Guid UserId = Guid.NewGuid();

        private static ChangePasswordCommand Cmd(
            string current = "Old1234!",
            string newPwd  = "New1234!",
            string confirm = "New1234!") =>
            new(UserId, current, newPwd, confirm);

        private static User AUser() => new() { Id = UserId, IsActive = true };

        // ── Contraseñas no coinciden ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PasswordsMismatch_ReturnsValidationFailed()
        {
            var result = await BuildSut().HandleAsync(Cmd(confirm: "Distinta!"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            await _identity.DidNotReceive().FindByIdAsync(Arg.Any<Guid>());
        }

        // ── Usuario no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsUserNotFound()
        {
            _identity.FindByIdAsync(UserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        // ── Contraseña actual incorrecta ─────────────────────────────────────

        [Fact]
        public async Task HandleAsync_IncorrectCurrentPassword_ReturnsInvalidCredentials()
        {
            _identity.FindByIdAsync(UserId).Returns(AUser());
            _identity.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
                     .Returns((false, new[] { "Incorrect password" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidCredentials);
        }

        // ── Fallo genérico de Identity ───────────────────────────────────────

        [Fact]
        public async Task HandleAsync_IdentityFailure_ReturnsValidationFailed()
        {
            _identity.FindByIdAsync(UserId).Returns(AUser());
            _identity.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
                     .Returns((false, new[] { "Password too short" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidRequest_ClearsForceChangeFlag()
        {
            var user = AUser();
            user.MustChangePassword = true;
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.ChangePasswordAsync(user, "Old1234!", "New1234!")
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.UpdateUserAsync(user)
                     .Returns((true, Enumerable.Empty<string>()));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            user.MustChangePassword.Should().BeFalse();
            await _identity.Received(1).UpdateUserAsync(user);
        }
    }
}
