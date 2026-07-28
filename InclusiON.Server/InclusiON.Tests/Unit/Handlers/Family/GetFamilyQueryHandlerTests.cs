using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class GetFamilyQueryHandlerTests
    {
        private readonly IFamilyRepository _repo = Substitute.For<IFamilyRepository>();

        private static readonly Guid FamilyId = Guid.NewGuid();

        private static FamilyRepresentative AFamily() => new()
        {
            Id = FamilyId, FirstName = "Carlos", LastName = "Paz",
            UserId = Guid.NewGuid(), User = new User { IsActive = true },
        };

        // ── GetFamilyById ────────────────────────────────────────────────────

        [Fact]
        public async Task GetFamilyById_NotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>())
                 .Returns((FamilyRepresentative?)null);

            var handler = new GetFamilyByIdQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetFamilyByIdQuery(FamilyId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task GetFamilyById_Found_ReturnsId()
        {
            _repo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>())
                 .Returns(AFamily());

            var handler = new GetFamilyByIdQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetFamilyByIdQuery(FamilyId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(FamilyId);
        }

        // ── GetFamily (paged) ────────────────────────────────────────────────

        [Fact]
        public async Task GetFamily_ReturnsMappedPagedResponse()
        {
            _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(), Arg.Any<List<int>?>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<FamilyRepresentative>
            {
                Data = new List<FamilyRepresentative> { AFamily() },
                TotalRecords = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10,
            });

            var handler = new GetFamilyQueryHandler(_repo);
            var result = await handler.HandleAsync(
                new GetFamilyQuery(1, 10, null, null, null, "asc"), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
            result.Data.Data.Should().HaveCount(1);
        }

        // ── GetFamilyLinkHistory ─────────────────────────────────────────────

        [Fact]
        public async Task GetFamilyLinkHistory_ReturnsMappedList()
        {
            _repo.GetPersonRepresentativeHistoryByFamilyAsync(FamilyId, Arg.Any<CancellationToken>())
                 .Returns(new List<PersonRepresentativeHistory>
                 {
                     new() { RepresentativeId = FamilyId },
                 });

            var handler = new GetFamilyLinkHistoryQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetFamilyLinkHistoryQuery(FamilyId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
        }

        // ── GetFamilyStatusHistory ───────────────────────────────────────────

        [Fact]
        public async Task GetFamilyStatusHistory_ReturnsMappedList()
        {
            _repo.GetFamilyStatusHistoryAsync(FamilyId, Arg.Any<CancellationToken>())
                 .Returns(new List<FamilyStatusHistory>
                 {
                     new() { FamilyId = FamilyId },
                 });

            var handler = new GetFamilyStatusHistoryQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetFamilyStatusHistoryQuery(FamilyId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
        }
    }
}
