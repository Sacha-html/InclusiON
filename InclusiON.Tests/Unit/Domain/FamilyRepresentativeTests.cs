using FluentAssertions;
using System.Reflection;
using Xunit;
using InclusiON.Domain.Models;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Tests.Unit.Domain
{
    /// <summary>
    /// Verifica el modelo de dominio <see cref="FamilyRepresentative"/>.
    ///
    /// El warning CS0108 ocurría porque la clase redeclaraba <c>IsActive</c> ocultando la propiedad
    /// heredada de <see cref="AuditableBaseEntity"/>. La redeclaración fue eliminada: la clase
    /// ahora usa la propiedad del base correctamente.
    /// </summary>
    public class FamilyRepresentativeTests
    {
        [Fact]
        public void IsActive_DeclaringType_IsBaseEntity_NotFamilyRepresentative()
        {
            // Arrange
            var prop = typeof(FamilyRepresentative)
                .GetProperty(nameof(FamilyRepresentative.IsActive));

            // Assert
            prop.Should().NotBeNull();
            prop!.DeclaringType.Should().Be(typeof(AuditableBaseEntity),
                because: "FamilyRepresentative hereda IsActive de AuditableBaseEntity " +
                          "y no debe redeclararla (evita CS0108 y comportamientos inesperados)");
        }

        [Fact]
        public void IsActive_DefaultValue_IsTrue()
        {
            // Arrange
            // La propiedad heredada ya tiene default = true; al quitar la redeclaración
            // el comportamiento debe ser idéntico.
            var representative = new FamilyRepresentative();

            // Assert
            representative.IsActive.Should().BeTrue(
                because: "AuditableBaseEntity.IsActive = true por defecto");
        }
    }
}
