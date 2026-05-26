using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Telemetry;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly IIdentityService     _identity  = Substitute.For<IIdentityService>();
        private readonly ILoginSessionService _sessions  = Substitute.For<ILoginSessionService>();
        private readonly ITelemetryService    _telemetry = Substitute.For<ITelemetryService>();

        private LoginCommandHandler BuildSut() =>
            new(_identity, _sessions, _telemetry,
                NullLogger<LoginCommandHandler>.Instance);

        private static readonly LoginCommand Cmd =
            new("user@test.com", "Password1!", RememberMe: false);

        private static User ActiveUser() => new()
        {
            Id       = Guid.NewGuid(),
            Email    = "user@test.com",
            IsActive = true,
        };

        private static ApiResponse<LoginResponse> SuccessSession() =>
            ApiResponse<LoginResponse>.SuccessResult(new LoginResponse
            {
                AccessToken  = "access",
                RefreshToken = "refresh",
                ExpiresAt    = DateTime.UtcNow.AddHours(1)
            });

        // ── Usuario no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsInvalidCredentials()
        {
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidCredentials);
        }

        // ── Cuenta inactiva ──────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserInactive_ReturnsAccountInactive()
        {
            var user = ActiveUser();
            user.IsActive = false;
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.AccountInactive);
        }

        // ── Cuenta bloqueada (lockout previo al intento) ─────────────────────

        [Fact]
        public async Task HandleAsync_AccountLockedOut_ReturnsAccountLocked()
        {
            var user = ActiveUser();
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(true);
            _identity.GetLockoutEndDateAsync(user).Returns(DateTimeOffset.UtcNow.AddMinutes(5));

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.AccountLocked);
        }

        // ── Contraseña incorrecta ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_WrongPassword_ReturnsInvalidCredentials()
        {
            var user = ActiveUser();
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, Arg.Any<string>(), Arg.Any<bool>())
                     .Returns(SignInStatus.Failed);

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidCredentials);
        }

        // ── Bloqueado por intentos fallidos (durante CheckPassword) ──────────

        [Fact]
        public async Task HandleAsync_LockedOutAfterFailure_ReturnsAccountLocked()
        {
            var user = ActiveUser();
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, Arg.Any<string>(), Arg.Any<bool>())
                     .Returns(SignInStatus.LockedOut);

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.AccountLocked);
        }

        // ── Rol no permitido ─────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_RoleNotAllowed_ReturnsRoleNotAllowedForLogin()
        {
            var user = ActiveUser();
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, Arg.Any<string>(), Arg.Any<bool>())
                     .Returns(SignInStatus.Success);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Professional" });

            var cmd    = new LoginCommand("user@test.com", "Password1!", AllowedRoles: ["Admin"]);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.RoleNotAllowedForLogin);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCredentials_ReturnsLoginResponse()
        {
            var user = ActiveUser();
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, Arg.Any<string>(), Arg.Any<bool>())
                     .Returns(SignInStatus.Success);
            _sessions.CreateLoginSessionAsync(user, Arg.Any<int>(), Arg.Any<string>(),
                      Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns(SuccessSession());

            var result = await BuildSut().HandleAsync(Cmd, default);

            result.Success.Should().BeTrue();
            result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        }
    }
}
