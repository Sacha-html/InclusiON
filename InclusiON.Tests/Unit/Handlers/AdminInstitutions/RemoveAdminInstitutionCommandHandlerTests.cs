using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminInstitutions
{
    public class RemoveAdminInstitutionCommandHandlerTests
    {
        private readonly IAdminInstitutionRepository _repository = Substitute.For<IAdminInstitutionRepository>();
        private readonly IUnitOfWork                 _uow        = Substitute.For<IUnitOfWork>();

        private RemoveAdminInstitutionCommandHandler BuildSut() => new(_repository, _uow);

        [Fact]
        public async Task HandleAsync_AssignmentNotFound_ReturnsNotFound()
        {
            _repository.FindAssignmentAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                       .Returns((AdminInstitution?)null);

            var result = await BuildSut().HandleAsync(
                new RemoveAdminInstitutionCommand(Guid.NewGuid(), 1), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
            _repository.DidNotReceive().Remove(Arg.Any<AdminInstitution>());
        }

        [Fact]
        public async Task HandleAsync_AssignmentExists_RemovesAndSaves()
        {
            var adminId    = Guid.NewGuid();
            var assignment = new AdminInstitution
            {
                AdminUserId   = adminId,
                InstitutionId = 3,
                Institution   = new EducationalInstitution { Id = 3, Name = "Centro N° 3" },
                AssignedAt    = DateTime.UtcNow,
                IsActive      = true
            };

            _repository.FindAssignmentAsync(adminId, 3, Arg.Any<CancellationToken>())
                       .Returns(assignment);

            var result = await BuildSut().HandleAsync(
                new RemoveAdminInstitutionCommand(adminId, 3), default);

            result.Success.Should().BeTrue();
            _repository.Received(1).Remove(assignment);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_Success_ResponseMapsAssignmentData()
        {
            var adminId    = Guid.NewGuid();
            var assignedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var assignment = new AdminInstitution
            {
                AdminUserId   = adminId,
                InstitutionId = 7,
                Institution   = new EducationalInstitution { Id = 7, Name = "Escuela Centro" },
                AssignedAt    = assignedAt,
                IsActive      = false
            };

            _repository.FindAssignmentAsync(adminId, 7, Arg.Any<CancellationToken>())
                       .Returns(assignment);

            var result = await BuildSut().HandleAsync(
                new RemoveAdminInstitutionCommand(adminId, 7), default);

            result.Data!.AdminUserId.Should().Be(adminId);
            result.Data.InstitutionId.Should().Be(7);
            result.Data.InstitutionName.Should().Be("Escuela Centro");
            result.Data.AssignedAt.Should().Be(assignedAt);
        }
    }
}
