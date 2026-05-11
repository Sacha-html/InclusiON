using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Handlers;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

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
            var list = new List<Invitation> { AnInvitation() };
            _repo.GetPagedByProfessionalIdAsync(ProfId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResponse<Invitation> { Data = list, TotalRecords = list.Count, TotalPages = 1, CurrentPage = 1, PageSize = 10 });

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
            await _repo.Received(1).GetPagedByProfessionalIdAsync(ProfId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        // ── Por instituciones ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ByInstitutionIds_CallsGetByInstitutions()
        {
            var ids = new List<int> { 1, 2 };
            var list2 = new List<Invitation> { AnInvitation(), AnInvitation() };
            _repo.GetPagedByInstitutionIdsAsync(ids, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResponse<Invitation> { Data = list2, TotalRecords = list2.Count, TotalPages = 1, CurrentPage = 1, PageSize = 10 });

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(null, ids), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(2);
            await _repo.Received(1).GetPagedByInstitutionIdsAsync(ids, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        // ── Todas ────────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoFilter_CallsGetAll()
        {
            _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResponse<Invitation>());

            var result = await BuildSut().HandleAsync(new GetInvitationsQuery(), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }
}
