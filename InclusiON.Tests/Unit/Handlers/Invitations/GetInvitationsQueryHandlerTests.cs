using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Handlers;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Invitations
{
    public class GetInvitationsQueryHandlerTests
    {
        private readonly IInvitationsRepository _repo = Substitute.For<IInvitationsRepository>();

        private GetInvitationsQueryHandler BuildSut() =>
            new(_repo, NullLogger<GetInvitationsQueryHandler>.Instance);

        private static readonly Guid ProfId = Guid.NewGuid();

        private static Invitation AnInvitation() => new()
        {
            Code = "abc", Email = "a@b.com", IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        // ── Por profesional ──────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ByProfessional_CallsGetByProfessional()
        {
            _repo.GetByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                 .Returns(new List<Invitation> { AnInvitation() });

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
            await _repo.Received(1).GetByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>());
        }

        // ── Por instituciones ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ByInstitutionIds_CallsGetByInstitutions()
        {
            var ids = new List<int> { 1, 2 };
            _repo.GetByInstitutionIdsAsync(ids, Arg.Any<CancellationToken>())
                 .Returns(new List<Invitation> { AnInvitation(), AnInvitation() });

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(null, ids), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(2);
            await _repo.Received(1).GetByInstitutionIdsAsync(ids, Arg.Any<CancellationToken>());
        }

        // ── Todas ────────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoFilter_CallsGetAll()
        {
            _repo.GetAllAsync(Arg.Any<CancellationToken>())
                 .Returns(new List<Invitation>());

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        }
    }
}
