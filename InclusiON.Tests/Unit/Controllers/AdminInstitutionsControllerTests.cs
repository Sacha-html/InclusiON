using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Constants;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Tests.Unit.Controllers
{
    /// <summary>
    /// Verifica las políticas de autorización en <see cref="AdminInstitutionsController"/>.
    ///
    /// La regla de negocio es:
    ///   - GET /me        → cualquier admin autenticado ve sus propias instituciones.
    ///   - GET /{adminId} → solo global-admin puede ver las instituciones de OTRO admin.
    ///
    /// Sin ese control un admin podría enumerar las asignaciones de todos los demás admins.
    /// </summary>
    public class AdminInstitutionsControllerTests
    {
        private static MethodInfo GetAction(string methodName) =>
            typeof(AdminInstitutionsController).GetMethod(methodName)
            ?? throw new InvalidOperationException($"Método '{methodName}' no encontrado.");

        // ── GET /{adminUserId} ── scope check ────────────────────────────────

        [Fact]
        public void GetAdminInstitutions_RequiresGlobalAdminPolicy()
        {
            // Arrange
            var method = GetAction(nameof(AdminInstitutionsController.GetAdminInstitutions));

            // Assert
            method.Should().BeDecoratedWith<AuthorizeAttribute>(a => a.Policy == Permissions.GlobalAdmin,
                because: "un admin no debe poder ver las instituciones asignadas a OTRO admin");
        }

        // ── GET /me ── acceso propio ──────────────────────────────────────────

        [Fact]
        public void GetMyInstitutions_DoesNotRequireGlobalAdminPolicy()
        {
            // Arrange
            // /me es el endpoint para que el propio admin vea sus instituciones.
            // Debe ser accesible con [Authorize] simple, no con global-admin.
            var method = GetAction(nameof(AdminInstitutionsController.GetMyInstitutions));

            // Act
            var authorizeAttrs = method
                .GetCustomAttributes<AuthorizeAttribute>()
                .ToList();

            // Assert
            // Tiene al menos un [Authorize]...
            authorizeAttrs.Should().NotBeEmpty();
            // ...pero ninguno exige global-admin.
            authorizeAttrs.Should().NotContain(a => a.Policy == Permissions.GlobalAdmin,
                because: "cualquier admin autenticado puede consultar sus propias instituciones");
        }

        // ── GET /admins, POST /users ── ya requerían global-admin ────────────

        [Fact]
        public void GetAllAdmins_RequiresGlobalAdminPolicy()
        {
            // Arrange
            var method = GetAction(nameof(AdminInstitutionsController.GetAllAdmins));

            // Assert
            method.Should().BeDecoratedWith<AuthorizeAttribute>(a => a.Policy == Permissions.GlobalAdmin);
        }

        [Fact]
        public void CreateAdminUser_RequiresGlobalAdminPolicy()
        {
            // Arrange
            var method = GetAction(nameof(AdminInstitutionsController.CreateAdminUser));

            // Assert
            method.Should().BeDecoratedWith<AuthorizeAttribute>(a => a.Policy == Permissions.GlobalAdmin);
        }
    }
}
