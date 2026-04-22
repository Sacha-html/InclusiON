using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Reflection;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.DTOs.Requests.Invitations;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;

namespace InclusiON.Tests.Unit.Controllers
{
    /// <summary>
    /// Verifica el enrutamiento de queries en <see cref="InvitationsController"/>:
    /// <list type="bullet">
    ///   <item>Profesional → filtra por su propio entityId (del JWT).</item>
    ///   <item>Admin institucional → filtra por institutionIds (del JWT).</item>
    ///   <item>GlobalAdmin → sin filtro.</item>
    ///   <item>CreateInvitation sin entityId → 404.</item>
    /// </list>
    /// </summary>
    public class InvitationsControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static InvitationsController BuildSut(
            Guid?       entityId       = null,
            List<int>?  institutionIds = null,
            string?     originHeader   = null)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            httpCtx.GetInstitutionIds().Returns(institutionIds ?? []);

            var authz = Substitute.For<IResourceAuthorizationService>();
            authz.CanAccessPersonAsync(
                    Arg.Any<Guid>(), Arg.Any<AccessMode>(), Arg.Any<CancellationToken>())
                 .Returns(true);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
                })
                .Build();

            var controller = new InvitationsController(httpCtx, authz, config);

            var httpContext = new DefaultHttpContext();
            if (originHeader != null)
                httpContext.Request.Headers["Origin"] = originHeader;

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        private static IQueryHandler<GetInvitationsQuery, ApiResponse<List<InvitationResponse>>> OkListHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetInvitationsQuery, ApiResponse<List<InvitationResponse>>>>();
            handler.HandleAsync(Arg.Any<GetInvitationsQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<List<InvitationResponse>>.SuccessResult([]));
            return handler;
        }

        private static ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>> FailingCreateHandler()
        {
            // Devuelve error para evitar el CreatedAtAction (que requiere routing real).
            // Lo que testamos es que HandleAsync fue llamado con el professionalId correcto.
            var handler = Substitute.For<ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>>>();
            handler.HandleAsync(Arg.Any<CreateInvitationCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<InvitationResponse>.ErrorResult("Test"));
            return handler;
        }

        // ── GetInvitations — enrutamiento por rol ────────────────────────────

        [Fact]
        public async Task GetInvitations_WithEntityId_FiltersToOwnInvitations()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var handler        = OkListHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.GetInvitations(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetInvitationsQuery>(q => q.ProfessionalId == professionalId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInvitations_WithoutEntityId_WithInstitutionIds_FiltersToInstitutions()
        {
            // Arrange
            var institutionIds = new List<int> { 2, 5 };
            var handler        = OkListHandler();
            var sut            = BuildSut(entityId: null, institutionIds: institutionIds);

            // Act
            await sut.GetInvitations(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetInvitationsQuery>(q =>
                    q.ProfessionalId == null
                    && q.InstitutionIds != null
                    && q.InstitutionIds.SequenceEqual(institutionIds)),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInvitations_GlobalAdmin_NoFilter()
        {
            // Arrange
            // Sin entityId y sin institutionIds → GlobalAdmin
            var handler = OkListHandler();
            var sut     = BuildSut(entityId: null, institutionIds: []);

            // Act
            await sut.GetInvitations(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetInvitationsQuery>(q =>
                    q.ProfessionalId == null
                    && (q.InstitutionIds == null || q.InstitutionIds.Count == 0)),
                Arg.Any<CancellationToken>());
        }

        // ── CreateInvitation ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateInvitation_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "test@test.com" },
                FailingCreateHandler());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateInvitation_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var handler        = FailingCreateHandler();
            var sut            = BuildSut(entityId: professionalId);

            // Act
            await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "test@test.com" },
                handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateInvitationCommand>(c => c.ProfessionalId == professionalId),
                Arg.Any<CancellationToken>());
        }

        // ── CreateInvitation — validación de Origin vs whitelist ─────────────
        // Si el Origin no viene de un dominio conocido, el link de invitación no puede
        // apuntar a un sitio de phishing controlado por el atacante.

        [Fact]
        public async Task CreateInvitation_OriginInWhitelist_UsesOriginAsBaseUrl()
        {
            // Arrange
            var professionalId = Guid.NewGuid();
            var handler        = FailingCreateHandler();
            var sut            = BuildSut(entityId: professionalId, originHeader: "http://localhost:4200");

            // Act
            await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "inv@test.com" },
                handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateInvitationCommand>(c => c.BaseUrl == "http://localhost:4200"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateInvitation_OriginNotInWhitelist_UsesFallbackOrigin()
        {
            // Arrange
            // Un atacante envía su propio dominio como Origin — debe ser ignorado.
            var professionalId = Guid.NewGuid();
            var handler        = FailingCreateHandler();
            var sut            = BuildSut(entityId: professionalId, originHeader: "https://attacker.com");

            // Act
            await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "inv@test.com" },
                handler);

            // Assert
            // El BaseUrl debe ser el primero de la whitelist, nunca el del atacante.
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateInvitationCommand>(c =>
                    c.BaseUrl == "http://localhost:4200"
                    && c.BaseUrl != "https://attacker.com"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateInvitation_NoOriginHeader_UsesFallbackOrigin()
        {
            // Arrange
            // Sin header Origin (ej: llamada directa desde Postman / otra API).
            var professionalId = Guid.NewGuid();
            var handler        = FailingCreateHandler();
            var sut            = BuildSut(entityId: professionalId, originHeader: null);

            // Act
            await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "inv@test.com" },
                handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateInvitationCommand>(c => c.BaseUrl == "http://localhost:4200"),
                Arg.Any<CancellationToken>());
        }

        // ── Rate limiting — endpoints públicos de invitaciones ────────────────
        // Estos endpoints son [AllowAnonymous]; sin rate limiting propio solo tienen
        // el global de 100 req/min, insuficiente para prevenir brute-force de códigos.

        [Fact]
        public void ValidateInvitation_HasRateLimitingPolicy()
        {
            // Arrange
            var method = typeof(InvitationsController)
                .GetMethod(nameof(InvitationsController.ValidateInvitation));

            // Assert
            method.Should().BeDecoratedWith<EnableRateLimitingAttribute>(
                a => a.PolicyName == "auth-sensitive",
                because: "un atacante podría enumerar códigos de invitación sin rate limiting");
        }

        [Fact]
        public void AcceptInvitation_HasRateLimitingPolicy()
        {
            // Arrange
            var method = typeof(InvitationsController)
                .GetMethod(nameof(InvitationsController.AcceptInvitation));

            // Assert
            method.Should().BeDecoratedWith<EnableRateLimitingAttribute>(
                a => a.PolicyName == "auth-sensitive",
                because: "el endpoint de registro anónimo requiere el mismo límite que signup");
        }
    }
}
