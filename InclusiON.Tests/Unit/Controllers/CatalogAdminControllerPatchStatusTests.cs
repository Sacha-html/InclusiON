using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;
using InclusiON.Infrastructure.Services;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Controllers
{
    public class CatalogAdminControllerPatchStatusTests : DbContextTestBase
    {
        private readonly IOutputCacheStore _cacheStore = Substitute.For<IOutputCacheStore>();

        public CatalogAdminControllerPatchStatusTests()
        {
            _cacheStore.EvictByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(ValueTask.CompletedTask);
        }

        private CatalogAdminController BuildSut() => new(new CatalogAdminService(Db), _cacheStore);

        private static DisabilityType ActiveDisabilityType(int id = 1) =>
            new() { Id = id, Name = "Motriz", IsActive = true };

        private static DisabilityType InactiveDisabilityType(int id = 1) =>
            new() { Id = id, Name = "Motriz", IsActive = false };

        private static AutonomyLevel ActiveAutonomyLevel(int id = 1) =>
            new() { Id = id, Name = "Alta", IsActive = true, DisplayOrder = 1 };

        private static AutonomyLevel InactiveAutonomyLevel(int id = 1) =>
            new() { Id = id, Name = "Alta", IsActive = false, DisplayOrder = 1 };

        private static PersonWithDisability PersonFor(int disabilityTypeId) =>
            new()
            {
                UserId         = Guid.NewGuid(),
                FirstName      = "Juan",
                LastName       = "Perez",
                BirthDate      = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DisabilityTypeId = disabilityTypeId,
            };

        // ── PatchDisabilityTypeStatus ────────────────────────────────────────

        [Fact]
        public async Task PatchDisabilityTypeStatus_NotFound_ReturnsNotFound()
        {
            // Arrange — DB vacía

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                99, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var body = ((NotFoundObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_AlreadyActive_ActivateRequested_ReturnsConflict()
        {
            // Arrange
            Db.DisabilityTypes.Add(ActiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                1, new PatchStatusRequest(true), default);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var body = ((ConflictObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_AlreadyInactive_DeactivateRequested_ReturnsConflict()
        {
            // Arrange
            Db.DisabilityTypes.Add(InactiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                1, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var body = ((ConflictObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_ActiveWithPersons_Deactivate_ReturnsConflict()
        {
            // Arrange
            Db.DisabilityTypes.Add(ActiveDisabilityType());
            Db.PersonsWithDisability.Add(PersonFor(disabilityTypeId: 1));
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                1, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var body = ((ConflictObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_ActiveNoPersons_Deactivate_ReturnsSuccess()
        {
            // Arrange
            Db.DisabilityTypes.Add(ActiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                1, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var body = ((OkObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.Success.Should().BeTrue();
            body.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_Deactivate_SetsIsActiveFalse()
        {
            // Arrange
            Db.DisabilityTypes.Add(ActiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            await BuildSut().PatchDisabilityTypeStatus(1, new PatchStatusRequest(false), default);

            // Assert
            var saved = await Db.DisabilityTypes.FindAsync(1);
            saved!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_Deactivate_EvictsCatalogCache()
        {
            // Arrange
            Db.DisabilityTypes.Add(ActiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            await BuildSut().PatchDisabilityTypeStatus(1, new PatchStatusRequest(false), default);

            // Assert
            await _cacheStore.Received(1).EvictByTagAsync("catalogs", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_Inactive_Activate_ReturnsSuccess()
        {
            // Arrange
            Db.DisabilityTypes.Add(InactiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchDisabilityTypeStatus(
                1, new PatchStatusRequest(true), default);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var body = ((OkObjectResult)result.Result!).Value as ApiResponse<CatalogItemResponse>;
            body!.Success.Should().BeTrue();
            body.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task PatchDisabilityTypeStatus_Activate_SetsIsActiveTrue()
        {
            // Arrange
            Db.DisabilityTypes.Add(InactiveDisabilityType());
            await Db.SaveChangesAsync();

            // Act
            await BuildSut().PatchDisabilityTypeStatus(1, new PatchStatusRequest(true), default);

            // Assert
            var saved = await Db.DisabilityTypes.FindAsync(1);
            saved!.IsActive.Should().BeTrue();
        }

        // ── PatchAutonomyLevelStatus (sin integrity check) ───────────────────

        [Fact]
        public async Task PatchAutonomyLevelStatus_NotFound_ReturnsNotFound()
        {
            // Arrange — DB vacía

            // Act
            var result = await BuildSut().PatchAutonomyLevelStatus(
                99, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var body = ((NotFoundObjectResult)result.Result!).Value as ApiResponse<AutonomyLevelResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task PatchAutonomyLevelStatus_NoOp_ReturnsConflict()
        {
            // Arrange
            Db.AutonomyLevels.Add(ActiveAutonomyLevel());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchAutonomyLevelStatus(
                1, new PatchStatusRequest(true), default);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var body = ((ConflictObjectResult)result.Result!).Value as ApiResponse<AutonomyLevelResponse>;
            body!.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task PatchAutonomyLevelStatus_Active_Deactivate_ReturnsSuccessAndSetsIsActiveFalse()
        {
            // Arrange
            Db.AutonomyLevels.Add(ActiveAutonomyLevel());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchAutonomyLevelStatus(
                1, new PatchStatusRequest(false), default);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var body = ((OkObjectResult)result.Result!).Value as ApiResponse<AutonomyLevelResponse>;
            body!.Success.Should().BeTrue();
            body.Data!.Id.Should().Be(1);
            var saved = await Db.AutonomyLevels.FindAsync(1);
            saved!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task PatchAutonomyLevelStatus_Inactive_Activate_ReturnsSuccessAndSetsIsActiveTrue()
        {
            // Arrange
            Db.AutonomyLevels.Add(InactiveAutonomyLevel());
            await Db.SaveChangesAsync();

            // Act
            var result = await BuildSut().PatchAutonomyLevelStatus(
                1, new PatchStatusRequest(true), default);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var body = ((OkObjectResult)result.Result!).Value as ApiResponse<AutonomyLevelResponse>;
            body!.Success.Should().BeTrue();
            body.Data!.Id.Should().Be(1);
            var saved = await Db.AutonomyLevels.FindAsync(1);
            saved!.IsActive.Should().BeTrue();
        }
    }
}
