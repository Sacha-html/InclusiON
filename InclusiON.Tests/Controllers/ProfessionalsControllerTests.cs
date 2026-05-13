using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Tests.Controllers
{
    /// <summary>
    /// Verifica que el endpoint <c>GET /api/professionals/me</c> resuelve el
    /// professionalId directamente desde el claim encriptado en el JWT,
    /// sin consulta adicional a base de datos.
    /// </summary>
    public class ProfessionalsControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static ProfessionalsController BuildSut(Guid? entityId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            return new ProfessionalsController(httpCtx);
        }

        private static IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>> OkHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>>>();
            handler.HandleAsync(Arg.Any<GetProfessionalByIdQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<ProfessionalResponse>.SuccessResult(new ProfessionalResponse()));
            return handler;
        }

        // ── GetMyProfile ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyProfile_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.GetMyProfile(OkHandler());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyProfile_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var handler        = OkHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.GetMyProfile(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetProfessionalByIdQuery>(q => q.ProfessionalId == professionalId),
                Arg.Any<CancellationToken>());
        }
    }
}
