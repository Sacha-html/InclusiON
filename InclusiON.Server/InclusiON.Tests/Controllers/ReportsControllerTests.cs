using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Reports;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;

namespace InclusiON.Tests.Controllers
{
    /// <summary>
    /// Verifica que <see cref="ReportsController"/> resuelve familyId / professionalId
    /// del claim encriptado en el JWT, sin consultas adicionales a BD.
    /// </summary>
    public class ReportsControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static ReportsController BuildSut(
            Guid? entityId,
            IResourceAuthorizationService? authz = null)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            httpCtx.GetCurrentUserRole().Returns(nameof(IdentityRoles.Professional));

            authz ??= Substitute.For<IResourceAuthorizationService>();
            authz.CanAccessPersonAsync(Arg.Any<Guid>(), Arg.Any<AccessMode>(), Arg.Any<CancellationToken>())
                 .Returns(true);

            return new ReportsController(httpCtx, authz);
        }

        private static IQueryHandler<GetFamilyReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>> OkFamilyReportsHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetFamilyReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>>>();
            handler.HandleAsync(Arg.Any<GetFamilyReportsQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<PagedResponse<ReportsListItemReponse>>.SuccessResult(
                       new PagedResponse<ReportsListItemReponse>()));
            return handler;
        }

        private static ICommandHandler<CreateReportCommand, ApiResponse<ReportResponse>> OkCreateHandler()
        {
            var handler = Substitute.For<ICommandHandler<CreateReportCommand, ApiResponse<ReportResponse>>>();
            handler.HandleAsync(Arg.Any<CreateReportCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<ReportResponse>.SuccessResult(new ReportResponse()));
            return handler;
        }

        private static ICommandHandler<UpdateReportCommand, ApiResponse<ReportResponse>> OkUpdateHandler()
        {
            var handler = Substitute.For<ICommandHandler<UpdateReportCommand, ApiResponse<ReportResponse>>>();
            handler.HandleAsync(Arg.Any<UpdateReportCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<ReportResponse>.SuccessResult(new ReportResponse()));
            return handler;
        }

        private static ICommandHandler<SubmitReportCommand, ApiResponse<ReportResponse>> OkSubmitHandler()
        {
            var handler = Substitute.For<ICommandHandler<SubmitReportCommand, ApiResponse<ReportResponse>>>();
            handler.HandleAsync(Arg.Any<SubmitReportCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<ReportResponse>.SuccessResult(new ReportResponse()));
            return handler;
        }

        private static GetReportsRequest DefaultPagedRequest() => new()
        {
            Page = 1, PageSize = 20, SortDirection = "DESC"
        };

        private static CreateReportRequest ValidCreateRequest() => new()
        {
            PersonId     = Guid.NewGuid(),
            Title        = "Informe inicial",
            Content      = "Contenido del informe",
            ReportTypeId = 1,
            ReportDate   = DateTime.UtcNow
        };

        private static UpdateReportRequest ValidUpdateRequest() => new()
        {
            Title        = "Informe actualizado",
            Content      = "Nuevo contenido",
            ReportTypeId = 1,
            ReportDate   = DateTime.UtcNow
        };

        // ── GetFamilyReports ────────────────────────────────────────────────

        [Fact]
        public async Task GetFamilyReports_NullEntityId_ReturnsBadRequest()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.GetFamilyReports(
                DefaultPagedRequest(), OkFamilyReportsHandler());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetFamilyReports_ValidEntityId_PassesFamilyIdToHandler()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var handler  = OkFamilyReportsHandler();
            var sut      = BuildSut(entityId: familyId);

            // Act
            await sut.GetFamilyReports(DefaultPagedRequest(), handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetFamilyReportsQuery>(q => q.FamilyRepresentativeId == familyId),
                Arg.Any<CancellationToken>());
        }

        // ── CreateReport ────────────────────────────────────────────────────

        [Fact]
        public async Task CreateReport_NullEntityId_ReturnsBadRequest()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.CreateReport(
                ValidCreateRequest(), OkCreateHandler());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateReport_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var request        = ValidCreateRequest();
            var handler        = OkCreateHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.CreateReport(request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateReportCommand>(c =>
                    c.ProfessionalId == professionalId && c.PersonId == request.PersonId),
                Arg.Any<CancellationToken>());
        }

        // ── UpdateReport ────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateReport_NullEntityId_ReturnsBadRequest()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.UpdateReport(
                1, ValidUpdateRequest(), OkUpdateHandler());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateReport_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var reportId       = 7;
            var handler        = OkUpdateHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.UpdateReport(reportId, ValidUpdateRequest(), handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<UpdateReportCommand>(c =>
                    c.ProfessionalId == professionalId && c.ReportId == reportId),
                Arg.Any<CancellationToken>());
        }

        // ── SubmitReport ────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitReport_NullEntityId_ReturnsBadRequest()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.SubmitReport(1, OkSubmitHandler());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SubmitReport_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var reportId       = 5;
            var handler        = OkSubmitHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.SubmitReport(reportId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<SubmitReportCommand>(c =>
                    c.ProfessionalId == professionalId && c.ReportId == reportId),
                Arg.Any<CancellationToken>());
        }
    }
}
