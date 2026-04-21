using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
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
            List<int>?  institutionIds = null)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            httpCtx.GetInstitutionIds().Returns(institutionIds ?? []);

            var authz = Substitute.For<IResourceAuthorizationService>();
            authz.CanAccessPersonAsync(
                    Arg.Any<Guid>(), Arg.Any<AccessMode>(), Arg.Any<CancellationToken>())
                 .Returns(true);

            var controller = new InvitationsController(httpCtx, authz);

            // DefaultHttpContext provee Request.Headers vacío — evita NPE en CreateInvitation.
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

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
            var professionalId = Guid.NewGuid();
            var handler        = OkListHandler();
            var sut            = BuildSut(entityId: professionalId);

            await sut.GetInvitations(handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<GetInvitationsQuery>(q => q.ProfessionalId == professionalId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInvitations_WithoutEntityId_WithInstitutionIds_FiltersToInstitutions()
        {
            var institutionIds = new List<int> { 2, 5 };
            var handler        = OkListHandler();
            var sut            = BuildSut(entityId: null, institutionIds: institutionIds);

            await sut.GetInvitations(handler);

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
            // Sin entityId y sin institutionIds → GlobalAdmin
            var handler = OkListHandler();
            var sut     = BuildSut(entityId: null, institutionIds: []);

            await sut.GetInvitations(handler);

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
            var sut = BuildSut(entityId: null);
            var result = await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "test@test.com" },
                FailingCreateHandler());

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateInvitation_ValidEntityId_PassesProfessionalIdToHandler()
        {
            var professionalId = Guid.NewGuid();
            var handler        = FailingCreateHandler();
            var sut            = BuildSut(entityId: professionalId);

            await sut.CreateInvitation(
                new CreateInvitationRequest { Email = "test@test.com" },
                handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<CreateInvitationCommand>(c => c.ProfessionalId == professionalId),
                Arg.Any<CancellationToken>());
        }
    }
}
