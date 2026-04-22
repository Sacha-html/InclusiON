using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using InclusiON.Api.Controllers;

namespace InclusiON.Tests.Unit.Controllers
{
    /// <summary>
    /// Verifica que <see cref="ProfessionalValidationController"/> esté protegido con
    /// <c>[Authorize]</c> y que no tenga <c>[AllowAnonymous]</c> a nivel de clase.
    ///
    /// Este endpoint permite verificar si un email o matrícula ya existe en el sistema.
    /// Si fuera anónimo, cualquier persona podría enumerar emails y matrículas registradas.
    /// </summary>
    public class ProfessionalValidationControllerTests
    {
        private static readonly Type ControllerType = typeof(ProfessionalValidationController);

        [Fact]
        public void Controller_HasAuthorizeAttribute()
        {
            ControllerType.Should().BeDecoratedWith<AuthorizeAttribute>(
                because: "el endpoint expone información de unicidad de emails y matrículas " +
                          "y solo debe ser accesible por usuarios autenticados");
        }

        [Fact]
        public void Controller_DoesNotHaveAllowAnonymousAttribute()
        {
            ControllerType.Should().NotBeDecoratedWith<AllowAnonymousAttribute>(
                because: "un [AllowAnonymous] a nivel de clase permitiría enumeración " +
                          "de emails y matrículas sin autenticación");
        }
    }
}
