using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Users.Handlers;
using InclusiON.Application.UseCases.Users.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Users
{
    public class GetUserProfileQueryHandlerTests
    {
        private readonly IIdentityService            _identity    = Substitute.For<IIdentityService>();
        private readonly IRefreshTokensRepository    _tokenRepo   = Substitute.For<IRefreshTokensRepository>();
        private readonly IPermissionService          _permissions = Substitute.For<IPermissionService>();
        private readonly IAdminInstitutionRepository _adminRepo   = Substitute.For<IAdminInstitutionRepository>();

        private GetUserProfileQueryHandler BuildSut() =>
            new(_identity, _tokenRepo, _permissions, _adminRepo);

        private static readonly Guid UserId = Guid.NewGuid();

        private static GetUserProfileQuery Query() => new(UserId);

        private static User ActiveUser() => new()
        {
            Id        = UserId,
            Name      = "Mirko",
            Surname   = "Dev",
            Email     = "mirko@test.com",
            IsActive  = true,
        };

        // ── Usuario no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsUserNotFound()
        {
            _identity.FindByIdAsync(UserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }

        // ── Cuenta inactiva ──────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UserInactive_ReturnsAccountInactive()
        {
            var user = ActiveUser();
            user.IsActive = false;
            _identity.FindByIdAsync(UserId).Returns(user);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.AccountInactive);
        }

        // ── Happy path: Professional ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalUser_ReturnsProfile()
        {
            var user = ActiveUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Professional" });
            _permissions.GetRolesPermissionsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(new List<string> { "reports.read" });
            _tokenRepo.GetActiveTokensCountAsync(UserId, Arg.Any<CancellationToken>()).Returns(2);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.Role.Should().Be("Professional");
            result.Data.ActiveSessionsCount.Should().Be(2);
            result.Data.IsGlobalAdmin.Should().BeNull();
            result.Data.InstitutionIds.Should().BeNull();
        }

        // ── Happy path: Admin global ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_GlobalAdmin_ReturnsIsGlobalAdminTrue()
        {
            var user = ActiveUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Admin" });
            _permissions.GetRolesPermissionsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(new List<string>());
            _tokenRepo.GetActiveTokensCountAsync(UserId, Arg.Any<CancellationToken>()).Returns(1);
            _adminRepo.GetActiveInstitutionIdsByAdminAsync(UserId, Arg.Any<CancellationToken>())
                      .Returns(new List<int>());

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.IsGlobalAdmin.Should().BeTrue();
            result.Data.InstitutionIds.Should().BeEmpty();
        }

        // ── Happy path: Admin institucional ──────────────────────────────────

        [Fact]
        public async Task HandleAsync_InstitutionalAdmin_ReturnsIsGlobalAdminFalse()
        {
            var user = ActiveUser();
            _identity.FindByIdAsync(UserId).Returns(user);
            _identity.GetRolesAsync(user).Returns(new List<string> { "Admin" });
            _permissions.GetRolesPermissionsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(new List<string>());
            _tokenRepo.GetActiveTokensCountAsync(UserId, Arg.Any<CancellationToken>()).Returns(1);
            _adminRepo.GetActiveInstitutionIdsByAdminAsync(UserId, Arg.Any<CancellationToken>())
                      .Returns(new List<int> { 1, 2 });

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.IsGlobalAdmin.Should().BeFalse();
            result.Data.InstitutionIds.Should().BeEquivalentTo(new[] { 1, 2 });
        }
    }
}
