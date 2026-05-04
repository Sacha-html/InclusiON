using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class AdminReactivateUserCommandHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IProfessionalsRepository _proRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository _personRepo = Substitute.For<IPersonsRepository>();
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IEmailService _email = Substitute.For<IEmailService>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private static readonly Guid TargetId = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private AdminReactivateUserCommandHandler BuildSut() =>
            new(_identity, _proRepo, _personRepo, _familyRepo, _email, _uow,
                NullLogger<AdminReactivateUserCommandHandler>.Instance, _dateTime);

        private static AdminReactivateUserCommand Cmd() => new(TargetId, AdminId);

        private static User InactiveUser() => new()
        {
            Id = TargetId, Email = "user@test.com", IsActive = false
        };

        private void SetupSuccessfulReset(User user)
        {
            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((true, Array.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);
        }

        [Fact]
        public async Task UserNotFound_ReturnsNotFound()
        {
            _identity.FindByIdAsync(TargetId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task AlreadyActive_ReturnsAlreadyActiveError()
        {
            var user = InactiveUser();
            user.IsActive = true;
            _identity.FindByIdAsync(TargetId).Returns(user);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserAlreadyActive);
        }

        [Fact]
        public async Task PasswordResetFails_ReturnsInternalError()
        {
            var user = InactiveUser();
            _identity.FindByIdAsync(TargetId).Returns(user);
            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((false, new[] { "Token inválido" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }

        [Fact]
        public async Task InactiveUser_NoLinkedEntity_ReactivatesAndSaves()
        {
            var user = InactiveUser();
            _identity.FindByIdAsync(TargetId).Returns(user);
            SetupSuccessfulReset(user);
            _identity.GetRolesAsync(user).Returns(new List<string>());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.UserEmail.Should().Be("user@test.com");
            user.IsActive.Should().BeTrue();
            user.MustChangePassword.Should().BeTrue();
            await _identity.Received(1).UpdateUserAsync(user);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InactiveProfessional_ReactivatesLinkedProfessional()
        {
            var user = InactiveUser();
            var innerUser = new User { Id = TargetId, IsActive = false };
            var pro = new Professional { Id = Guid.NewGuid(), UserId = TargetId, User = innerUser };

            _identity.FindByIdAsync(TargetId).Returns(user);
            SetupSuccessfulReset(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Professional" });
            _proRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(pro);

            await BuildSut().HandleAsync(Cmd(), default);

            pro.User.IsActive.Should().BeTrue();
            await _proRepo.Received(1).UpdateAsync(pro, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InactivePerson_ReactivatesLinkedPerson()
        {
            var user = InactiveUser();
            var innerUser = new User { Id = TargetId, IsActive = false };
            var person = new PersonWithDisability
            {
                Id = Guid.NewGuid(), UserId = TargetId, User = innerUser,
                BirthDate = new DateTime(2000, 1, 1)
            };

            _identity.FindByIdAsync(TargetId).Returns(user);
            SetupSuccessfulReset(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "PersonWithDisability" });
            _personRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(person);

            await BuildSut().HandleAsync(Cmd(), default);

            person.User.IsActive.Should().BeTrue();
            await _personRepo.Received(1).UpdateAsync(person, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InactiveFamily_ReactivatesLinkedFamily()
        {
            var user = InactiveUser();
            var innerUser = new User { Id = TargetId, IsActive = false };
            var family = new FamilyRepresentative
                { Id = Guid.NewGuid(), UserId = TargetId, User = innerUser };

            _identity.FindByIdAsync(TargetId).Returns(user);
            SetupSuccessfulReset(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "FamilyRepresentative" });
            _familyRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(family);

            await BuildSut().HandleAsync(Cmd(), default);

            family.User.IsActive.Should().BeTrue();
            await _familyRepo.Received(1).UpdateAsync(family, Arg.Any<CancellationToken>());
        }
    }
}
