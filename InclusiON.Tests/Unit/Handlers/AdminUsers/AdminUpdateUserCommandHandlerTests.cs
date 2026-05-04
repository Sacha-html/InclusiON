using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class AdminUpdateUserCommandHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

        private AdminUpdateUserCommandHandler BuildSut() =>
            new(_identity, NullLogger<AdminUpdateUserCommandHandler>.Instance);

        private static Guid UserId()      => new("aaaaaaaa-0000-0000-0000-000000000001");
        private static Guid OtherId()     => new("bbbbbbbb-0000-0000-0000-000000000002");

        private static AdminUpdateUserCommand ValidCommand(
            Guid? userId    = null,
            Guid? requestedBy = null,
            string email    = "admin@test.com") =>
            new(
                userId      ?? UserId(),
                requestedBy ?? UserId(),
                "María",
                "López",
                email);

        private static User ExistingUser(Guid? id = null, string email = "admin@test.com") =>
            new()
            {
                Id       = id ?? UserId(),
                Name     = "Viejo",
                Surname  = "Nombre",
                Email    = email,
                UserName = email,
                NormalizedEmail    = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                IsActive = true,
            };

        // ── Autorización ────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DifferentUser_ReturnsForbidden()
        {
            // Arrange
            var cmd = ValidCommand(userId: UserId(), requestedBy: OtherId());

            // Act
            var result = await BuildSut().HandleAsync(cmd, default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
            await _identity.DidNotReceive().FindByIdAsync(Arg.Any<Guid>());
        }

        // ── Usuario no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _identity.FindByIdAsync(UserId()).Returns((User?)null);

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Happy path — mismo email ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_SameEmail_UpdatesNameAndSurname_ReturnsSuccess()
        {
            // Arrange
            var user = ExistingUser();
            _identity.FindByIdAsync(UserId()).Returns(user);
            _identity.UpdateUserAsync(Arg.Any<User>())
                     .Returns((true, Enumerable.Empty<string>()));

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeTrue();
            user.Name.Should().Be("María");
            user.Surname.Should().Be("López");
            // Email no cambió → FindByEmailAsync no debe llamarse
            await _identity.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
        }

        // ── Happy path — email nuevo ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NewEmail_NotTaken_UpdatesEmailFields_ReturnsSuccess()
        {
            // Arrange
            var user = ExistingUser(email: "viejo@test.com");
            _identity.FindByIdAsync(UserId()).Returns(user);
            _identity.FindByEmailAsync("nuevo@test.com").Returns((User?)null);
            _identity.UpdateUserAsync(Arg.Any<User>())
                     .Returns((true, Enumerable.Empty<string>()));

            var cmd = ValidCommand(email: "nuevo@test.com");

            // Act
            var result = await BuildSut().HandleAsync(cmd, default);

            // Assert
            result.Success.Should().BeTrue();
            user.Email.Should().Be("nuevo@test.com");
            user.UserName.Should().Be("nuevo@test.com");
            user.NormalizedEmail.Should().Be("NUEVO@TEST.COM");
            user.NormalizedUserName.Should().Be("NUEVO@TEST.COM");
        }

        // ── Conflicto de email ───────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NewEmail_AlreadyTaken_ReturnsConflict()
        {
            // Arrange
            var user  = ExistingUser(email: "viejo@test.com");
            var other = ExistingUser(id: OtherId(), email: "ocupado@test.com");
            _identity.FindByIdAsync(UserId()).Returns(user);
            _identity.FindByEmailAsync("ocupado@test.com").Returns(other);

            var cmd = ValidCommand(email: "ocupado@test.com");

            // Act
            var result = await BuildSut().HandleAsync(cmd, default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
            await _identity.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
        }

        // ── Fallo de Identity ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UpdateUserFails_ReturnsInternalError()
        {
            // Arrange
            var user = ExistingUser();
            _identity.FindByIdAsync(UserId()).Returns(user);
            _identity.UpdateUserAsync(Arg.Any<User>())
                     .Returns((false, (IEnumerable<string>)["PasswordTooShort"]));

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }
    }
}
