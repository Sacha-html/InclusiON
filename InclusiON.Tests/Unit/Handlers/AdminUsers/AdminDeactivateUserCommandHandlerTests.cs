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
    public class AdminDeactivateUserCommandHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IRefreshTokensRepository _tokens = Substitute.For<IRefreshTokensRepository>();
        private readonly IProfessionalsRepository _proRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository _personRepo = Substitute.For<IPersonsRepository>();
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IAccessAuditLogger _audit = Substitute.For<IAccessAuditLogger>();

        private static readonly Guid TargetId = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private AdminDeactivateUserCommandHandler BuildSut() =>
            new(_identity, _tokens, _proRepo, _personRepo, _familyRepo, _uow,
                NullLogger<AdminDeactivateUserCommandHandler>.Instance, _audit);

        private static AdminDeactivateUserCommand Cmd(Guid? target = null, Guid? admin = null) =>
            new(target ?? TargetId, admin ?? AdminId);

        private static User ActiveUser() => new()
        {
            Id = TargetId, Email = "user@test.com", IsActive = true
        };

        [Fact]
        public async Task SelfDeactivation_ReturnsSelfDeactivateError()
        {
            var result = await BuildSut().HandleAsync(Cmd(target: AdminId, admin: AdminId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.CannotDeactivateSelf);
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
        public async Task AlreadyInactive_ReturnsAlreadyInactiveError()
        {
            var user = ActiveUser();
            user.IsActive = false;
            _identity.FindByIdAsync(TargetId).Returns(user);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserAlreadyInactive);
        }

        [Fact]
        public async Task ActiveUser_NoLinkedEntity_DeactivatesAndSaves()
        {
            var user = ActiveUser();
            _identity.FindByIdAsync(TargetId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string>());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            user.IsActive.Should().BeFalse();
            await _identity.Received(1).UpdateUserAsync(user);
            await _tokens.Received(1).RevokeAllUserTokensAsync(
                TargetId, Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ActiveProfessional_DeactivatesLinkedProfessional()
        {
            var user = ActiveUser();
            var innerUser = new User { Id = TargetId, IsActive = true };
            var pro = new Professional { Id = Guid.NewGuid(), UserId = TargetId, User = innerUser };

            _identity.FindByIdAsync(TargetId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Professional" });
            _proRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(pro);

            await BuildSut().HandleAsync(Cmd(), default);

            pro.User.IsActive.Should().BeFalse();
            await _proRepo.Received(1).UpdateAsync(pro, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ActivePerson_DeactivatesLinkedPerson()
        {
            var user = ActiveUser();
            var innerUser = new User { Id = TargetId, IsActive = true };
            var person = new PersonWithDisability
            {
                Id = Guid.NewGuid(), UserId = TargetId, User = innerUser,
                BirthDate = new DateTime(2000, 1, 1)
            };

            _identity.FindByIdAsync(TargetId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "PersonWithDisability" });
            _personRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(person);

            await BuildSut().HandleAsync(Cmd(), default);

            person.User.IsActive.Should().BeFalse();
            await _personRepo.Received(1).UpdateAsync(person, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ActiveFamily_DeactivatesLinkedFamily()
        {
            var user = ActiveUser();
            var innerUser = new User { Id = TargetId, IsActive = true };
            var family = new FamilyRepresentative
                { Id = Guid.NewGuid(), UserId = TargetId, User = innerUser };

            _identity.FindByIdAsync(TargetId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "FamilyRepresentative" });
            _familyRepo.GetByUserIdAsync(TargetId, Arg.Any<CancellationToken>()).Returns(family);

            await BuildSut().HandleAsync(Cmd(), default);

            family.User.IsActive.Should().BeFalse();
            await _familyRepo.Received(1).UpdateAsync(family, Arg.Any<CancellationToken>());
        }
    }
}
