using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Tests.Controllers
{
    public class ValidationControllerTests
    {
        [Fact]
        public void Controller_HasAuthorizeAttribute_AndNoAllowAnonymous()
        {
            var type = typeof(ValidationController);

            type.Should().BeDecoratedWith<AuthorizeAttribute>(
                because: "los endpoints de validación en vivo no deben permitir enumeración anónima de DNI, email o matrícula");
            type.Should().NotBeDecoratedWith<AllowAnonymousAttribute>();
        }

        [Fact]
        public async Task CheckDocument_WhenUnavailable_ReturnsUnavailableWithMessage()
        {
            var service = Substitute.For<IUniquenessValidationService>();
            service.CheckDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Taken("Alumno", true, "Ya existe un alumno inactivo."));

            var controller = new ValidationController(service);
            var actionResult = await controller.CheckDocument("12345678", null, default);

            var ok = actionResult.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<FieldAvailabilityResponse>().Subject;
            payload.IsAvailable.Should().BeFalse();
            payload.IsInactiveStudent.Should().BeTrue();
            payload.Message.Should().Contain("inactivo");
        }
    }
}
