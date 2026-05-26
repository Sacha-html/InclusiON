using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class UpdateLoginMethodCommandHandlerTests
    {
        private readonly IVisualLoginRepository _repo = Substitute.For<IVisualLoginRepository>();
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IPinHasher _pinHasher = Substitute.For<IPinHasher>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid SupervisorUserId = Guid.NewGuid();

        private UpdateLoginMethodCommandHandler BuildSut() =>
            new(_repo, _identity, _pinHasher, _uow,
                NullLogger<UpdateLoginMethodCommandHandler>.Instance);

        private static UpdateLoginMethodCommand Cmd(int loginMethodId = 1, string? pin = null, Guid? supervisorId = null) =>
            new(UserId, loginMethodId, pin, supervisorId);

        private static PersonWithDisability APerson() => new()
        {
            Id = Guid.NewGuid(), UserId = UserId, BirthDate = new DateTime(2000, 1, 1)
        };

        private static LoginMethod ActiveMethod(int id = 1, string name = "Standard") => new()
        {
            Id = id, Name = name, IsActive = true
        };

        [Fact]
        public async Task PersonNotFound_ReturnsPersonNotFound()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task LoginMethodNotFound_ReturnsResourceNotFound()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((LoginMethod?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ResourceNotFound);
        }

        [Fact]
        public async Task LoginMethodInactive_ReturnsLoginMethodNotAllowed()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(new LoginMethod { Id = 1, Name = "Standard", IsActive = false });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.LoginMethodNotAllowed);
        }

        [Fact]
        public async Task PinMethod_NullPin_ReturnsRequiredField()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(2, Arg.Any<CancellationToken>()).Returns(ActiveMethod(2, "PIN"));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 2, pin: null), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.RequiredField);
        }

        [Fact]
        public async Task PinMethod_InvalidFormat_ReturnsInvalidFormat()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(2, Arg.Any<CancellationToken>()).Returns(ActiveMethod(2, "PIN"));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 2, pin: "abc"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidFormat);
        }

        [Fact]
        public async Task PinMethod_ValidPin_UpdatesAndReturnsSuccess()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(2, Arg.Any<CancellationToken>()).Returns(ActiveMethod(2, "PIN"));
            _pinHasher.Hash("1234").Returns("hashed1234");

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 2, pin: "1234"), default);

            result.Success.Should().BeTrue();
            result.Data!.LoginMethodId.Should().Be(2);
            await _repo.Received(1).UpdatePersonLoginMethodAsync(
                UserId, 2, "hashed1234", null, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AssistedMethod_NoSupervisor_ReturnsRequiredField()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(3, Arg.Any<CancellationToken>()).Returns(ActiveMethod(3, "Assisted"));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 3, supervisorId: null), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.RequiredField);
        }

        [Fact]
        public async Task AssistedMethod_SupervisorNotFound_ReturnsSupervisorNotAuthorized()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(3, Arg.Any<CancellationToken>()).Returns(ActiveMethod(3, "Assisted"));
            _repo.GetProfessionalByUserIdAsync(SupervisorUserId, Arg.Any<CancellationToken>())
                .Returns((Professional?)null);
            _repo.GetFamilyByUserIdAsync(SupervisorUserId, Arg.Any<CancellationToken>())
                .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 3, supervisorId: SupervisorUserId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.SupervisorNotAuthorized);
        }

        [Fact]
        public async Task AssistedMethod_ValidSupervisor_UpdatesAndReturnsSuccess()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(3, Arg.Any<CancellationToken>()).Returns(ActiveMethod(3, "Asistido"));
            _repo.GetProfessionalByUserIdAsync(SupervisorUserId, Arg.Any<CancellationToken>())
                .Returns(new Professional { Id = Guid.NewGuid(), UserId = SupervisorUserId });
            _repo.GetFamilyByUserIdAsync(SupervisorUserId, Arg.Any<CancellationToken>())
                .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 3, supervisorId: SupervisorUserId), default);

            result.Success.Should().BeTrue();
            result.Data!.LoginMethodId.Should().Be(3);
            await _repo.Received(1).UpdatePersonLoginMethodAsync(
                UserId, 3, null, SupervisorUserId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task StandardMethod_UserNotFoundInIdentity_ReturnsPersonNotFound()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveMethod(1, "Standard"));
            _identity.FindByIdAsync(UserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 1), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task StandardMethod_ResetFails_ReturnsValidationFailed()
        {
            var user = new User { Id = UserId };
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveMethod(1, "Standard"));
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((false, (IEnumerable<string>)new[] { "Token inválido" }));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 1), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task StandardMethod_Valid_ReturnsSuccessWithTemporaryPassword()
        {
            var user = new User { Id = UserId };
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveMethod(1, "Standard"));
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((true, Array.Empty<string>()));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 1), default);

            result.Success.Should().BeTrue();
            result.Data!.TemporaryPassword.Should().NotBeNullOrEmpty();
            user.MustChangePassword.Should().BeTrue();
            await _identity.Received(1).UpdateUserAsync(user);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UnknownMethod_ReturnsLoginMethodNotAllowed()
        {
            _repo.GetPersonByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(APerson());
            _repo.GetLoginMethodByIdAsync(99, Arg.Any<CancellationToken>()).Returns(ActiveMethod(99, "Unknown"));

            var result = await BuildSut().HandleAsync(Cmd(loginMethodId: 99), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.LoginMethodNotAllowed);
        }
    }
}
