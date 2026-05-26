using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminInstitutions
{
    public class AssignInstitutionToAdminCommandHandlerTests
    {
        private readonly IAdminInstitutionRepository _adminRepo      = Substitute.For<IAdminInstitutionRepository>();
        private readonly IInstitutionsRepository     _institutionRepo = Substitute.For<IInstitutionsRepository>();
        private readonly IIdentityService            _identity        = Substitute.For<IIdentityService>();
        private readonly IUnitOfWork                 _uow             = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider           _dateTime        = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService          _encryption      = Substitute.For<IEncryptionService>();

        private AssignInstitutionToAdminCommandHandler BuildSut() =>
            new(_adminRepo, _institutionRepo, _identity, _uow, _dateTime, _encryption);

        private static EducationalInstitution AnInstitution(int id = 1) =>
            new() { Id = id, Name = "Escuela N° 1" };

        private static User AUser(Guid? id = null) =>
            new() { Id = id ?? Guid.NewGuid() };

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _identity.FindByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

            // Act
            var result = await BuildSut().HandleAsync(
                new AssignInstitutionToAdminCommand(Guid.NewGuid(), 1), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_InstitutionNotFound_ReturnsNotFound()
        {
            // Arrange
            _identity.FindByIdAsync(Arg.Any<Guid>()).Returns(AUser());
            _institutionRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                            .Returns((EducationalInstitution?)null);

            // Act
            var result = await BuildSut().HandleAsync(
                new AssignInstitutionToAdminCommand(Guid.NewGuid(), 99), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_NewAssignment_AddsAndSaves()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            _identity.FindByIdAsync(adminId).Returns(AUser(adminId));
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _adminRepo.FindAssignmentAsync(adminId, 1, Arg.Any<CancellationToken>())
                      .Returns((AdminInstitution?)null);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            var result = await BuildSut().HandleAsync(
                new AssignInstitutionToAdminCommand(adminId, 1), default);

            // Assert
            result.Success.Should().BeTrue();
            await _adminRepo.Received(1).AddAsync(
                Arg.Is<AdminInstitution>(ai => ai.AdminUserId == adminId && ai.InstitutionId == 1 && ai.IsActive),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_ExistingActiveAssignment_DoesNotSaveAgain()
        {
            // Arrange
            var adminId    = Guid.NewGuid();
            var existing   = new AdminInstitution { AdminUserId = adminId, InstitutionId = 1, IsActive = true, Institution = AnInstitution() };
            _identity.FindByIdAsync(adminId).Returns(AUser(adminId));
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _adminRepo.FindAssignmentAsync(adminId, 1, Arg.Any<CancellationToken>()).Returns(existing);

            // Act
            var result = await BuildSut().HandleAsync(
                new AssignInstitutionToAdminCommand(adminId, 1), default);

            // Assert
            result.Success.Should().BeTrue();
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_ExistingInactiveAssignment_ReactivatesAndSaves()
        {
            // Arrange
            var adminId  = Guid.NewGuid();
            var existing = new AdminInstitution { AdminUserId = adminId, InstitutionId = 1, IsActive = false, Institution = AnInstitution() };
            _identity.FindByIdAsync(adminId).Returns(AUser(adminId));
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _adminRepo.FindAssignmentAsync(adminId, 1, Arg.Any<CancellationToken>()).Returns(existing);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            var result = await BuildSut().HandleAsync(
                new AssignInstitutionToAdminCommand(adminId, 1), default);

            // Assert
            result.Success.Should().BeTrue();
            existing.IsActive.Should().BeTrue();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
