using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class VisualLoginCommandHandlersTests
    {
        private readonly IVisualLoginRepository _repo = Substitute.For<IVisualLoginRepository>();
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly ILoginSessionService _sessions = Substitute.For<ILoginSessionService>();
        private readonly IPinHasher _pinHasher = Substitute.For<IPinHasher>();

        private static readonly Guid UserId = Guid.NewGuid();

        private static User AUser() => new() { Id = UserId, Email = "person@test.com", IsActive = true };

        private static PersonWithDisability APerson(User? user = null) => new()
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            FirstName = "Ana",
            LastName = "García",
            User = user ?? AUser(),
            BirthDate = new DateTime(2000, 1, 1),
        };

        private static FamilyRepresentative AFamily(User? user = null) => new()
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            FirstName = "María",
            LastName = "Ruiz",
            User = user ?? AUser(),
        };

        private static ApiResponse<VisualLoginResponse> SuccessSession() =>
            ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse { Success = true, AccessToken = "tok" });

        // ════════════════════════════════════════════════════════════════
        // PinLoginCommandHandler
        // ════════════════════════════════════════════════════════════════

        private PinLoginCommandHandler BuildPin() =>
            new(_repo, _identity, _pinHasher, _sessions);

        private static PinLoginCommand PinCmd(string pin = "1234") =>
            new(UserId, pin, DeviceId: null, RememberDevice: false);

        [Fact]
        public async Task Pin_PersonNotFound_ReturnsUserNotFound()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns((PersonWithDisability?)null);

            var result = await BuildPin().HandleAsync(PinCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        [Fact]
        public async Task Pin_AccountLocked_ReturnsLockedResponse()
        {
            var user = AUser();
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns(APerson(user));
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(true);
            _identity.GetLockoutEndDateAsync(user)
                .Returns(DateTimeOffset.UtcNow.AddMinutes(5));

            var result = await BuildPin().HandleAsync(PinCmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Success.Should().BeFalse();
            result.Data.IsLocked.Should().BeTrue();
        }

        [Fact]
        public async Task Pin_NoPinConfigured_ReturnsPinNotConfigured()
        {
            var user = AUser();
            var person = APerson(user);
            person.PinCodeHash = null;

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);

            var result = await BuildPin().HandleAsync(PinCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PinNotConfigured);
        }

        [Fact]
        public async Task Pin_WrongPin_ReturnsRemainingAttempts()
        {
            var user = AUser();
            var person = APerson(user);
            person.PinCodeHash = "hash";

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);

            bool needsRehash;
            _pinHasher.Verify("hash", "0000", out needsRehash).Returns(false);

            _identity.GetAccessFailedCountAsync(user).Returns(2);

            var result = await BuildPin().HandleAsync(PinCmd("0000"), default);

            result.Success.Should().BeTrue();
            result.Data!.Success.Should().BeFalse();
            result.Data.RemainingAttempts.Should().Be(3);
        }

        [Fact]
        public async Task Pin_CorrectPin_CreatesSession()
        {
            var user = AUser();
            var person = APerson(user);
            person.PinCodeHash = "hash";

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.IsLockedOutAsync(user).Returns(false);

            bool needsRehash;
            _pinHasher.Verify("hash", "1234", out needsRehash).Returns(true);

            _sessions.CreateVisualLoginSessionAsync(
                    user, person, Arg.Any<int>(), Arg.Any<string?>(),
                    Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .Returns(SuccessSession());

            var result = await BuildPin().HandleAsync(PinCmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Success.Should().BeTrue();
        }

        // ════════════════════════════════════════════════════════════════
        // VisualStandardLoginCommandHandler
        // ════════════════════════════════════════════════════════════════

        private VisualStandardLoginCommandHandler BuildVisualStandard() =>
            new(_repo, _identity, _sessions);

        private static VisualStandardLoginCommand VisualCmd(string pwd = "pass") =>
            new(UserId, pwd);

        [Fact]
        public async Task VisualStandard_ReturnsNotAvailableError()
        {
            var result = await BuildVisualStandard().HandleAsync(VisualCmd(), default);

            result.Data!.Success.Should().BeFalse();
            result.Data.ErrorMessage.Should().Contain("no está disponible para alumnos");
        }

        // ════════════════════════════════════════════════════════════════
        // FamilyLoginCommandHandler
        // ════════════════════════════════════════════════════════════════

        private FamilyLoginCommandHandler BuildFamily() =>
            new(_repo, _identity, _sessions);

        private static FamilyLoginCommand FamilyCmd(string pwd = "pass") =>
            new(UserId, pwd);

        [Fact]
        public async Task Family_FamilyNotFound_ReturnsUserNotFound()
        {
            _repo.GetFamilyByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns((FamilyRepresentative?)null);

            var result = await BuildFamily().HandleAsync(FamilyCmd(), default);

            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        [Fact]
        public async Task Family_AccountLocked_ReturnsLockedResponse()
        {
            var user = AUser();
            _repo.GetFamilyByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(AFamily(user));
            _identity.IsLockedOutAsync(user).Returns(true);
            _identity.GetLockoutEndDateAsync(user).Returns(DateTimeOffset.UtcNow.AddMinutes(5));

            var result = await BuildFamily().HandleAsync(FamilyCmd(), default);

            result.Data!.IsLocked.Should().BeTrue();
        }

        [Fact]
        public async Task Family_WrongPassword_LockoutAfterCheck_ReturnsLocked()
        {
            var user = AUser();
            _repo.GetFamilyByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(AFamily(user));
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, "bad", lockoutOnFailure: true)
                .Returns(SignInStatus.LockedOut);
            _identity.GetLockoutEndDateAsync(user).Returns(DateTimeOffset.UtcNow.AddMinutes(10));

            var result = await BuildFamily().HandleAsync(FamilyCmd("bad"), default);

            result.Data!.IsLocked.Should().BeTrue();
        }

        [Fact]
        public async Task Family_CorrectPassword_CreatesSession()
        {
            var user = AUser();
            var family = AFamily(user);
            _repo.GetFamilyByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(family);
            _identity.IsLockedOutAsync(user).Returns(false);
            _identity.CheckPasswordAsync(user, "pass", lockoutOnFailure: true)
                .Returns(SignInStatus.Success);
            _sessions.CreateFamilyLoginSessionAsync(
                    user, family, Arg.Any<int>(), Arg.Any<string?>(),
                    Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .Returns(SuccessSession());

            var result = await BuildFamily().HandleAsync(FamilyCmd(), default);

            result.Data!.Success.Should().BeTrue();
        }

        // ════════════════════════════════════════════════════════════════
        // AssistedLoginCommandHandler
        // ════════════════════════════════════════════════════════════════

        private AssistedLoginCommandHandler BuildAssisted() =>
            new(_repo, _identity, _sessions,
                NullLogger<AssistedLoginCommandHandler>.Instance);

        private static AssistedLoginCommand AssistedCmd(string supEmail = "sup@test.com", string supPwd = "pass") =>
            new(UserId, supEmail, supPwd);

        private static User ASupervisor() => new()
        {
            Id = Guid.NewGuid(),
            Email = "sup@test.com",
            IsActive = true
        };

        [Fact]
        public async Task Assisted_PersonNotFound_ReturnsUserNotFound()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns((PersonWithDisability?)null);

            var result = await BuildAssisted().HandleAsync(AssistedCmd(), default);

            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        [Fact]
        public async Task Assisted_SupervisorNotFound_ReturnsFailure()
        {
            var person = APerson();
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByEmailAsync("sup@test.com").Returns((User?)null);

            var result = await BuildAssisted().HandleAsync(AssistedCmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Assisted_SupervisorNotAuthorized_ReturnsNotAuthorized()
        {
            var user = AUser();
            var person = APerson(user);
            person.SupervisorUserId = null;

            var supervisor = ASupervisor();
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByEmailAsync("sup@test.com").Returns(supervisor);
            // No direct supervisor match, check roles
            _identity.FindByIdAsync(supervisor.Id).Returns(supervisor);
            _identity.GetRolesAsync(supervisor).Returns(new List<string>());

            var result = await BuildAssisted().HandleAsync(AssistedCmd(), default);

            result.Data!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Assisted_DirectSupervisor_CorrectPassword_CreatesSession()
        {
            var user = AUser();
            var supervisor = ASupervisor();
            var person = APerson(user);
            person.SupervisorUserId = supervisor.Id; // direct supervisor

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByEmailAsync("sup@test.com").Returns(supervisor);
            _identity.CheckPasswordAsync(supervisor, "pass", lockoutOnFailure: true)
                .Returns(SignInStatus.Success);
            _sessions.CreateVisualLoginSessionAsync(
                    user, person, Arg.Any<int>(), Arg.Any<string?>(),
                    Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .Returns(SuccessSession());

            var result = await BuildAssisted().HandleAsync(AssistedCmd(), default);

            result.Data!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Assisted_DirectSupervisor_WrongPassword_ReturnsFailure()
        {
            var user = AUser();
            var supervisor = ASupervisor();
            var person = APerson(user);
            person.SupervisorUserId = supervisor.Id;

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByEmailAsync("sup@test.com").Returns(supervisor);
            _identity.CheckPasswordAsync(supervisor, "wrong", lockoutOnFailure: true)
                .Returns(SignInStatus.Failed);

            var result = await BuildAssisted().HandleAsync(AssistedCmd(supPwd: "wrong"), default);

            result.Data!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Assisted_DirectSupervisor_LockedOut_ReturnsLocked()
        {
            var user = AUser();
            var supervisor = ASupervisor();
            var person = APerson(user);
            person.SupervisorUserId = supervisor.Id;

            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(person);
            _identity.FindByEmailAsync("sup@test.com").Returns(supervisor);
            _identity.CheckPasswordAsync(supervisor, "pass", lockoutOnFailure: true)
                .Returns(SignInStatus.LockedOut);

            var result = await BuildAssisted().HandleAsync(AssistedCmd(), default);

            result.Data!.Success.Should().BeFalse();
            result.Data.IsLocked.Should().BeTrue();
        }
    }
}
