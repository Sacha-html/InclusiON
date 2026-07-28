using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Handlers;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminInstitutions
{
    public class GetAdminInstitutionsQueryHandlerTests
    {
        private readonly IAdminInstitutionRepository _repository = Substitute.For<IAdminInstitutionRepository>();
        private readonly IEncryptionService          _encryption = Substitute.For<IEncryptionService>();
        private GetAdminInstitutionsQueryHandler BuildSut() => new(_repository, _encryption);

        [Fact]
        public async Task HandleAsync_AdminWithNoAssignments_ReturnsEmptyList()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            _repository.GetInstitutionsByAdminAsync(adminId, Arg.Any<CancellationToken>())
                       .Returns([]);

            // Act
            var result = await BuildSut().HandleAsync(new GetAdminInstitutionsQuery(adminId), default);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_MapsAssignmentFieldsCorrectly()
        {
            // Arrange
            var adminId     = Guid.NewGuid();
            var assignedAt  = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            var institution = new EducationalInstitution { Id = 5, Name = "Escuela Cervantes" };
            var assignment  = new AdminInstitution
            {
                AdminUserId   = adminId,
                InstitutionId = 5,
                Institution   = institution,
                AssignedAt    = assignedAt,
                IsActive      = true
            };

            _repository.GetInstitutionsByAdminAsync(adminId, Arg.Any<CancellationToken>())
                       .Returns([assignment]);

            // Act
            var result = await BuildSut().HandleAsync(new GetAdminInstitutionsQuery(adminId), default);

            // Assert
            var dto = result.Data!.Data.Single();
            dto.AdminUserId.Should().Be(adminId);
            dto.InstitutionId.Should().Be(5);
            dto.InstitutionName.Should().Be("Escuela Cervantes");
            dto.AssignedAt.Should().Be(assignedAt);
            dto.IsActive.Should().BeTrue();
        }
    }
}
