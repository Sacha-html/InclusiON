using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class ReactivateFamilyCommandHandlerTests
    {
        private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IBackgroundJobRepository _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private static readonly Guid FamilyId = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private ReactivateFamilyCommandHandler BuildSut()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.ArgAt<Func<CancellationToken, Task>>(0)(callInfo.ArgAt<CancellationToken>(1)));

            return new(_familyRepo, _identity, _backgroundJobs, _uow,
                NullLogger<ReactivateFamilyCommandHandler>.Instance, _dateTime);
        }

        private static FamilyRepresentative InactiveFamily() => new()
        {
            Id = FamilyId,
            UserId = Guid.NewGuid(),
            FirstName = "María",
            LastName = "López",
            IsActive = false,
            Status = FamilyStatusEnum.Terminated,
            User = new User
            {
                IsActive = false,
                Email = "maria@test.com",
                AccessFailedCount = 3,
                LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5)
            },
            PersonRepresentatives = new List<PersonRepresentative>
            {
                new()
                {
                    IsActive = false,
                    Person = new PersonWithDisability
                    {
                        IsActive = false,
                        User = new User { IsActive = false },
                        BirthDate = new DateTime(2010, 1, 1)
                    }
                }
            }
        };

        [Fact]
        public async Task HandleAsync_FamilyNotFound_ReturnsNotFound()
        {
            _familyRepo.GetByIdForUpdateAsync(FamilyId, Arg.Any<CancellationToken>())
                .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(new ReactivateFamilyCommand(FamilyId, AdminId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_AlreadyActiveFamily_ReturnsBusinessRuleViolation()
        {
            var family = InactiveFamily();
            family.IsActive = true;
            family.User.IsActive = true;
            _familyRepo.GetByIdForUpdateAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);

            var result = await BuildSut().HandleAsync(new ReactivateFamilyCommand(FamilyId, AdminId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task HandleAsync_InactiveFamily_ReactivatesOnlyFamilyAccountAndNotifies()
        {
            var family = InactiveFamily();
            var inactiveLink = family.PersonRepresentatives.Single();
            var now = new DateTime(2026, 9, 6, 12, 0, 0, DateTimeKind.Utc);
            _familyRepo.GetByIdForUpdateAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _identity.ResetPasswordAsync(family.User, Arg.Any<string>()).Returns((true, Array.Empty<string>()));
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(new ReactivateFamilyCommand(FamilyId, AdminId), default);

            result.Success.Should().BeTrue();
            result.Data!.TemporaryPassword.Should().NotBeNullOrEmpty();
            family.IsActive.Should().BeTrue();
            family.Status.Should().Be(FamilyStatusEnum.Active);
            family.User.IsActive.Should().BeTrue();
            family.User.MustChangePassword.Should().BeTrue();
            family.User.LockoutEnd.Should().BeNull();
            family.User.AccessFailedCount.Should().Be(0);
            family.UpdatedAt.Should().Be(now);
            inactiveLink.IsActive.Should().BeFalse();
            inactiveLink.Person.IsActive.Should().BeFalse();
            inactiveLink.Person.User!.IsActive.Should().BeFalse();
            await _identity.Received(1).UpdateUserAsync(family.User);
            await _familyRepo.Received(1).CreateFamilyStatusHistoryAsync(
                Arg.Is<FamilyStatusHistory>(h => h.FamilyId == FamilyId
                    && h.OldStatus == FamilyStatusEnum.Terminated
                    && h.NewStatus == FamilyStatusEnum.Active
                    && h.ChangedByUserId == AdminId),
                Arg.Any<CancellationToken>());
            await _backgroundJobs.Received(1).CreateAsync(
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime?>(), 2, Arg.Any<CancellationToken>());
            await _uow.Received(1).ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
