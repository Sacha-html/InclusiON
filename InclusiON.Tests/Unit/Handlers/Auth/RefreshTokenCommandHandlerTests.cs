using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Telemetry;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly IIdentityService          _identity   = Substitute.For<IIdentityService>();
        private readonly IRefreshTokensRepository  _tokenRepo  = Substitute.For<IRefreshTokensRepository>();
        private readonly ILoginSessionService      _sessions   = Substitute.For<ILoginSessionService>();
        private readonly ITelemetryService         _telemetry  = Substitute.For<ITelemetryService>();
        private readonly IDateTimeProvider         _dateTime   = Substitute.For<IDateTimeProvider>();

        private RefreshTokenCommandHandler BuildSut() =>
            new(_identity, _tokenRepo, _sessions, _telemetry,
                NullLogger<RefreshTokenCommandHandler>.Instance, _dateTime);

        private static RefreshTokenCommand Cmd(string token = "valid-token") =>
            new(token);

        private static RefreshToken ActiveToken(DateTime? expiresAt = null) => new()
        {
            Token     = "valid-token",
            UserId    = Guid.NewGuid(),
            IsActive  = true,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
        };

        private static User ActiveUser(Guid userId) => new()
        {
            Id       = userId,
            Email    = "user@test.com",
            IsActive = true,
        };

        private static ApiResponse<LoginResponse> SuccessSession() =>
            ApiResponse<LoginResponse>.SuccessResult(new LoginResponse
            {
                AccessToken  = "new-access",
                RefreshToken = "new-refresh",
                ExpiresAt    = DateTime.UtcNow.AddHours(1)
            });

        // ── Token vacío ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task HandleAsync_EmptyToken_ReturnsRequiredField(string? token)
        {
            var result = await BuildSut().HandleAsync(new RefreshTokenCommand(token!), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.RequiredField);
        }

        // ── Token no encontrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_TokenNotFound_ReturnsTokenInvalid()
        {
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>())
                      .Returns((RefreshToken?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.TokenInvalid);
        }

        // ── Token revocado ───────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_TokenRevoked_ReturnsTokenInvalid()
        {
            var token = ActiveToken();
            token.IsActive = false;
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(token);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.TokenInvalid);
        }

        // ── Token expirado ───────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_TokenExpired_ReturnsTokenExpired()
        {
            var now   = DateTime.UtcNow;
            var token = ActiveToken(expiresAt: now.AddDays(-1));
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(token);
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.TokenExpired);
            await _tokenRepo.Received(1).RevokeAsync("valid-token", Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        // ── Usuario no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsUserNotFound()
        {
            var now   = DateTime.UtcNow;
            var token = ActiveToken(expiresAt: now.AddDays(7));
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(token);
            _dateTime.UtcNow.Returns(now);
            _identity.FindByIdAsync(token.UserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        // ── Usuario inactivo ─────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserInactive_ReturnsAccountInactive()
        {
            var now   = DateTime.UtcNow;
            var token = ActiveToken(expiresAt: now.AddDays(7));
            var user  = ActiveUser(token.UserId);
            user.IsActive = false;
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(token);
            _dateTime.UtcNow.Returns(now);
            _identity.FindByIdAsync(token.UserId).Returns(user);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.AccountInactive);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidToken_ReturnsNewLoginResponse()
        {
            var now   = DateTime.UtcNow;
            var token = ActiveToken(expiresAt: now.AddDays(7));
            var user  = ActiveUser(token.UserId);
            _tokenRepo.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(token);
            _dateTime.UtcNow.Returns(now);
            _identity.FindByIdAsync(token.UserId).Returns(user);
            _sessions.CreateLoginSessionAsync(user, Arg.Any<int>(), Arg.Any<string>(),
                      Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns(SuccessSession());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("new-access");
        }
    }
}
