using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class AdminResetPasswordCommandHandlerTests
    {
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IRefreshTokensRepository _tokens = Substitute.For<IRefreshTokensRepository>();
        private readonly IBackgroundJobRepository _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IAdminInstitutionRepository _adminInstitRepo = Substitute.For<IAdminInstitutionRepository>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IAccessAuditLogger _audit = Substitute.For<IAccessAuditLogger>();

        private static readonly Guid TargetUserId = Guid.NewGuid();
        private static readonly Guid AdminUserId = Guid.NewGuid();

        private AdminResetPasswordCommandHandler BuildSut() =>
            new(_identity, _tokens, _backgroundJobs, _uow, _adminInstitRepo,
                NullLogger<AdminResetPasswordCommandHandler>.Instance, _dateTime, _audit);

        private static AdminResetPasswordCommand Cmd() =>
            new(TargetUserId, AdminUserId);

        private static User AUser() => new()
        {
            Id = TargetUserId,
            Email = "target@test.com",
            IsActive = true
        };

        [Fact]
        public async Task UserNotFound_ReturnsNotFound()
        {
            _identity.FindByIdAsync(TargetUserId).Returns((User?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task GlobalAdmin_ResetSucceeds_WithoutScopeCheck()
        {
            var user = AUser();
            _identity.FindByIdAsync(TargetUserId).Returns(user);

            // Global admin = empty institution list
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(AdminUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int>());

            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((true, Array.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.UserEmail.Should().Be("target@test.com");
            result.Data!.TemporaryPassword.Should().NotBeNullOrEmpty();
            await _identity.Received(1).UpdateUserAsync(user);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            user.MustChangePassword.Should().BeTrue();
        }

        [Fact]
        public async Task InstitutionalAdmin_TargetInSameInstitution_ResetSucceeds()
        {
            var user = AUser();
            _identity.FindByIdAsync(TargetUserId).Returns(user);

            // Requesting admin has institution 5
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(AdminUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int> { 5 });

            // Target user also belongs to institution 5 → overlap
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(TargetUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int> { 5 });

            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((true, Array.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task InstitutionalAdmin_TargetInDifferentInstitution_ReturnsForbidden()
        {
            var user = AUser();
            _identity.FindByIdAsync(TargetUserId).Returns(user);

            // Requesting admin has institution 5
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(AdminUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int> { 5 });

            // Target belongs to institution 99 → no overlap
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(TargetUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int> { 99 });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task InstitutionalAdmin_TargetHasNoInstitutions_ResetSucceeds()
        {
            // Target with no institutions (e.g. global admin being reset by institutional admin)
            // The handler only blocks when target has institutions AND no overlap.
            var user = AUser();
            _identity.FindByIdAsync(TargetUserId).Returns(user);

            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(AdminUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int> { 5 });

            // Target has no institutions → skip overlap check
            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(TargetUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int>());

            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((true, Array.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task PasswordResetFails_ReturnsInternalError()
        {
            var user = AUser();
            _identity.FindByIdAsync(TargetUserId).Returns(user);

            _adminInstitRepo.GetActiveInstitutionIdsByAdminAsync(AdminUserId, Arg.Any<CancellationToken>())
                .Returns(new List<int>());

            _identity.ResetPasswordAsync(user, Arg.Any<string>())
                .Returns((false, new[] { "Token inválido" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }
    }
}
