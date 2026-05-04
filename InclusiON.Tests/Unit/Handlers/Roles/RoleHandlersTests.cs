using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roles.Commands;
using InclusiON.Application.UseCases.Roles.Handlers;
using InclusiON.Application.UseCases.Roles.Queries;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Roles
{
    // ════════════════════════════════════════════════════════════════════════════
    // GetRolesQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetRolesQueryHandlerTests
    {
        private readonly IRoleService _roleService = Substitute.For<IRoleService>();

        private GetRolesQueryHandler BuildSut() => new(_roleService);

        [Fact]
        public async Task HandleAsync_NoRoles_ReturnsEmptyList()
        {
            _roleService.GetAllAsync(Arg.Any<CancellationToken>())
                        .Returns(new List<RoleDto>());

            var result = await BuildSut().HandleAsync(new GetRolesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_WithRoles_MapsToRoleResponse()
        {
            var roleId = Guid.NewGuid();
            _roleService.GetAllAsync(Arg.Any<CancellationToken>())
                        .Returns(new List<RoleDto>
                        {
                            new(roleId, "Profesional", "PROFESIONAL", new List<string> { "persons:read", "activities:read" })
                        });

            var result = await BuildSut().HandleAsync(new GetRolesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].Id.Should().Be(roleId);
            result.Data[0].Name.Should().Be("Profesional");
            result.Data[0].Permissions.Should().BeEquivalentTo("persons:read", "activities:read");
        }

        [Fact]
        public async Task HandleAsync_MultipleRoles_ReturnsAll()
        {
            _roleService.GetAllAsync(Arg.Any<CancellationToken>())
                        .Returns(new List<RoleDto>
                        {
                            new(Guid.NewGuid(), "Admin",      "ADMIN",      new List<string> { "settings:update" }),
                            new(Guid.NewGuid(), "Profesional","PROFESIONAL",new List<string> { "persons:read" }),
                            new(Guid.NewGuid(), "Familiar",   "FAMILIAR",   new List<string>())
                        });

            var result = await BuildSut().HandleAsync(new GetRolesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(3);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetRoleByIdQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetRoleByIdQueryHandlerTests
    {
        private readonly IRoleService _roleService = Substitute.For<IRoleService>();

        private GetRoleByIdQueryHandler BuildSut() => new(_roleService);

        private static readonly Guid RoleId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_RoleNotFound_ReturnsNotFound()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns((RoleDto?)null);

            var result = await BuildSut().HandleAsync(new GetRoleByIdQuery(RoleId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_RoleFound_ReturnsMappedResponse()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new RoleDto(RoleId, "Profesional", "PROFESIONAL",
                                             new List<string> { "persons:read", "reports:read" }));

            var result = await BuildSut().HandleAsync(new GetRoleByIdQuery(RoleId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(RoleId);
            result.Data.Name.Should().Be("Profesional");
            result.Data.Permissions.Should().BeEquivalentTo("persons:read", "reports:read");
        }

        [Fact]
        public async Task HandleAsync_RoleWithNoPermissions_ReturnsEmptyPermissions()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new RoleDto(RoleId, "Familiar", "FAMILIAR", new List<string>()));

            var result = await BuildSut().HandleAsync(new GetRoleByIdQuery(RoleId), default);

            result.Success.Should().BeTrue();
            result.Data!.Permissions.Should().BeEmpty();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // UpdateRolePermissionsCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class UpdateRolePermissionsCommandHandlerTests
    {
        private readonly IRoleService             _roleService   = Substitute.For<IRoleService>();
        private readonly IRefreshTokensRepository _tokenRepo     = Substitute.For<IRefreshTokensRepository>();

        private UpdateRolePermissionsCommandHandler BuildSut() =>
            new(_roleService, _tokenRepo,
                NullLogger<UpdateRolePermissionsCommandHandler>.Instance);

        private static readonly Guid RoleId = Guid.NewGuid();

        private static RoleDto ARole(List<string>? permissions = null) =>
            new(RoleId, "Profesional", "PROFESIONAL", permissions ?? new List<string>());

        // ── Rol no encontrado ───────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_RoleNotFound_ReturnsNotFound()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns((RoleDto?)null);

            var cmd    = new UpdateRolePermissionsCommand(RoleId, new[] { "persons:read" });
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_RoleNotFound_NeverCallsUpdatePermissions()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns((RoleDto?)null);

            await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, new[] { "persons:read" }), default);

            await _roleService.DidNotReceive()
                .UpdatePermissionsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }

        // ── UpdatePermissions devuelve false (rol desapareció entre las dos llamadas) ─

        [Fact]
        public async Task HandleAsync_UpdateReturnsFalse_ReturnsNotFound()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(ARole());
            _roleService.UpdatePermissionsAsync(RoleId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(false);

            var result = await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, new[] { "persons:read" }), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Sin usuarios en el rol ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoUsersInRole_NeverRevokesTokens()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(ARole());
            _roleService.UpdatePermissionsAsync(RoleId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(true);
            _roleService.GetUserIdsByRoleAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new List<Guid>());

            await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, new[] { "persons:read" }), default);

            await _tokenRepo.DidNotReceive()
                .RevokeAllUsersTokensAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        // ── Con usuarios en el rol ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_WithUsersInRole_RevokesTheirTokens()
        {
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(ARole());
            _roleService.UpdatePermissionsAsync(RoleId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(true);
            _roleService.GetUserIdsByRoleAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new List<Guid> { userId1, userId2 });
            _tokenRepo.RevokeAllUsersTokensAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                      .Returns(3);

            var result = await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, new[] { "persons:read" }), default);

            result.Success.Should().BeTrue();
            await _tokenRepo.Received(1)
                .RevokeAllUsersTokensAsync(
                    Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(userId1) && ids.Contains(userId2)),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>());
        }

        // ── Respuesta con permisos deduplicados y ordenados ─────────────────────

        [Fact]
        public async Task HandleAsync_Success_ReturnsPermissionsDedupedAndSorted()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(ARole());
            _roleService.UpdatePermissionsAsync(RoleId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(true);
            _roleService.GetUserIdsByRoleAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new List<Guid>());

            var permissions = new[] { "persons:read", "activities:read", "persons:read" }; // duplicado
            var result      = await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, permissions), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(RoleId);
            result.Data.Name.Should().Be("Profesional");
            // Sin duplicados y ordenados
            result.Data.Permissions.Should().BeInAscendingOrder();
            result.Data.Permissions.Should().OnlyHaveUniqueItems();
            result.Data.Permissions.Should().BeEquivalentTo("persons:read", "activities:read");
        }

        // ── Permisos vacíos (revocar todos) ────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmptyPermissions_UpdatesWithEmptyList()
        {
            _roleService.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(ARole(new List<string> { "persons:read" }));
            _roleService.UpdatePermissionsAsync(RoleId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                        .Returns(true);
            _roleService.GetUserIdsByRoleAsync(RoleId, Arg.Any<CancellationToken>())
                        .Returns(new List<Guid>());

            var result = await BuildSut().HandleAsync(
                new UpdateRolePermissionsCommand(RoleId, Enumerable.Empty<string>()), default);

            result.Success.Should().BeTrue();
            result.Data!.Permissions.Should().BeEmpty();

            await _roleService.Received(1)
                .UpdatePermissionsAsync(
                    RoleId,
                    Arg.Is<IEnumerable<string>>(p => !p.Any()),
                    Arg.Any<CancellationToken>());
        }
    }
}
