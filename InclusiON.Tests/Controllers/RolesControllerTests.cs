using FluentAssertions;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Roles.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Roles;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;

namespace InclusiON.Tests.Controllers
{
    public class RolesControllerTests
    {
        private static RolesController BuildSut(IMemoryCache cache)
            => new RolesController(cache, Substitute.For<IOutputCacheStore>());

        // ── UpdateRolePermissions ────────────────────────────────────────────

        [Fact]
        public async Task UpdateRolePermissions_HandlerSuccess_InvalidatesRolePermissionsCache()
        {
            // Arrange
            var cache   = Substitute.For<IMemoryCache>();
            var sut     = BuildSut(cache);
            var handler = Substitute.For<ICommandHandler<UpdateRolePermissionsCommand, ApiResponse<RoleResponse>>>();
            handler.HandleAsync(Arg.Any<UpdateRolePermissionsCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<RoleResponse>.SuccessResult(new RoleResponse { Name = "Professional" }));
            var roleId  = Guid.NewGuid();
            var request = new UpdateRolePermissionsRequest { Permissions = new List<string>() };
            var ct      = CancellationToken.None;

            // Act
            await sut.UpdateRolePermissions(roleId, request, handler, ct);

            // Assert
            cache.Received(1).Remove("RolePermissions_PROFESSIONAL");
        }

        [Fact]
        public async Task UpdateRolePermissions_HandlerFailure_DoesNotInvalidateCache()
        {
            // Arrange
            var cache   = Substitute.For<IMemoryCache>();
            var sut     = BuildSut(cache);
            var handler = Substitute.For<ICommandHandler<UpdateRolePermissionsCommand, ApiResponse<RoleResponse>>>();
            handler.HandleAsync(Arg.Any<UpdateRolePermissionsCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<RoleResponse>.ErrorResult(ErrorCode.NotFound, "Role not found"));
            var roleId  = Guid.NewGuid();
            var request = new UpdateRolePermissionsRequest { Permissions = new List<string>() };
            var ct      = CancellationToken.None;

            // Act
            await sut.UpdateRolePermissions(roleId, request, handler, ct);

            // Assert
            cache.DidNotReceive().Remove(Arg.Any<object>());
        }
    }
}
