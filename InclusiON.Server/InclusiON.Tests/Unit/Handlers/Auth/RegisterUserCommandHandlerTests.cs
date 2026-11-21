using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly IIdentityService             _identity   = Substitute.For<IIdentityService>();
        private readonly IUniquenessValidationService _uniqueness = Substitute.For<IUniquenessValidationService>();
        private readonly IDateTimeProvider            _dateTime   = Substitute.For<IDateTimeProvider>();

        public RegisterUserCommandHandlerTests()
        {
            _uniqueness.CheckEmailAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Available());
        }

        private RegisterUserCommandHandler BuildSut() => new(_identity, _uniqueness, _dateTime);

        private static RegisterUserCommand Cmd(
            string password  = "Password1!",
            string confirm   = "Password1!") =>
            new("Juan", "García", "juan@test.com", password, confirm,
                PhoneNumber: null, Role: IdentityRoles.Professional);

        // ── Contraseñas no coinciden ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PasswordsMismatch_ReturnsValidationFailed()
        {
            var result = await BuildSut().HandleAsync(Cmd(confirm: "Diferente!"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            await _uniqueness.DidNotReceive().CheckEmailAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        // ── Email ya registrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailAlreadyExists_ReturnsConflict()
        {
            _uniqueness.CheckEmailAsync("juan@test.com", null, null, false, Arg.Any<CancellationToken>())
                       .Returns(EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema."));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Error de Identity al crear ───────────────────────────────────────

        [Fact]
        public async Task HandleAsync_CreateFails_ReturnsValidationFailed()
        {
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((false, new[] { "Password too weak" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidRequest_CreatesUserAndAssignsRole()
        {
            var now = DateTime.UtcNow;
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Email.Should().Be("juan@test.com");
            await _identity.Received(1).AddToRoleAsync(
                Arg.Any<User>(),
                Arg.Is<string>(r => r == IdentityRoles.Professional.ToString()));
        }
    }
}
