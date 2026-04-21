using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.DTOs.Requests.Diagnoses;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Tests.Unit.Controllers
{
    /// <summary>
    /// Verifica que <see cref="DiagnosesController"/> lee el professionalId
    /// directamente del claim encriptado en el JWT (via <see cref="IHttpContextService.GetCurrentEntityId"/>)
    /// sin realizar una consulta adicional a base de datos.
    /// </summary>
    public class DiagnosesControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static DiagnosesController BuildSut(Guid? entityId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            return new DiagnosesController(httpCtx);
        }

        private static ICommandHandler<CreateDiagnosisCommand, ApiResponse<DiagnosisResponse>> OkCreateHandler()
        {
            var handler = Substitute.For<ICommandHandler<CreateDiagnosisCommand, ApiResponse<DiagnosisResponse>>>();
            handler.HandleAsync(Arg.Any<CreateDiagnosisCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<DiagnosisResponse>.SuccessResult(new DiagnosisResponse()));
            return handler;
        }

        private static ICommandHandler<UpdateDiagnosisCommand, ApiResponse<DiagnosisResponse>> OkUpdateHandler()
        {
            var handler = Substitute.For<ICommandHandler<UpdateDiagnosisCommand, ApiResponse<DiagnosisResponse>>>();
            handler.HandleAsync(Arg.Any<UpdateDiagnosisCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<DiagnosisResponse>.SuccessResult(new DiagnosisResponse()));
            return handler;
        }

        private static CreateDiagnosisRequest ValidCreateRequest() => new()
        {
            DiagnosisDate    = DateTime.UtcNow,
            PrimaryDiagnosis = "TEA nivel 2"
        };

        private static UpdateDiagnosisRequest ValidUpdateRequest() => new()
        {
            DiagnosisDate    = DateTime.UtcNow,
            PrimaryDiagnosis = "TEA nivel 2"
        };

        // ── CreateDiagnosis ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateDiagnosis_NullEntityId_ReturnsBadRequest()
        {
            var sut    = BuildSut(entityId: null);
            var result = await sut.CreateDiagnosis(
                Guid.NewGuid(), ValidCreateRequest(), OkCreateHandler());

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateDiagnosis_ValidEntityId_PassesProfessionalIdToHandler()
        {
            var professionalId = Guid.NewGuid();
            var personId       = Guid.NewGuid();
            var handler        = OkCreateHandler();
            var sut            = BuildSut(entityId: professionalId);

            await sut.CreateDiagnosis(personId, ValidCreateRequest(), handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<CreateDiagnosisCommand>(c =>
                    c.ProfessionalId == professionalId && c.PersonId == personId),
                Arg.Any<CancellationToken>());
        }

        // ── UpdateDiagnosis ─────────────────────────────────────────────────

        [Fact]
        public async Task UpdateDiagnosis_NullEntityId_ReturnsBadRequest()
        {
            var sut    = BuildSut(entityId: null);
            var result = await sut.UpdateDiagnosis(
                1, ValidUpdateRequest(), OkUpdateHandler());

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateDiagnosis_ValidEntityId_PassesProfessionalIdToHandler()
        {
            var professionalId = Guid.NewGuid();
            var diagnosisId    = 42;
            var handler        = OkUpdateHandler();
            var sut            = BuildSut(entityId: professionalId);

            await sut.UpdateDiagnosis(diagnosisId, ValidUpdateRequest(), handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<UpdateDiagnosisCommand>(c =>
                    c.RequestedByProfessionalId == professionalId && c.DiagnosisId == diagnosisId),
                Arg.Any<CancellationToken>());
        }
    }
}
