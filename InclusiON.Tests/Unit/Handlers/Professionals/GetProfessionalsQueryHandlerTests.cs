using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class GetProfessionalsQueryHandlerTests
    {
        private readonly IProfessionalsRepository  _prosRepo   = Substitute.For<IProfessionalsRepository>();
        private readonly IAdminInstitutionRepository _adminRepo = Substitute.For<IAdminInstitutionRepository>();
        private readonly IHttpContextService        _httpCtx    = Substitute.For<IHttpContextService>();

        private static readonly Guid ProfId  = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private static PagedResponse<Professional> OnePagedPro() => new()
        {
            Data = new List<Professional> { new() { Id = ProfId } },
            TotalRecords = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10,
        };

        // ── GetProfessionalById ──────────────────────────────────────────────

        [Fact]
        public async Task GetProfessionalById_NotFound_ReturnsProfessionalNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var handler = new GetProfessionalByIdQueryHandler(_prosRepo);
            var result = await handler.HandleAsync(new GetProfessionalByIdQuery(ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        [Fact]
        public async Task GetProfessionalById_Found_ReturnsProfile()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(new Professional { Id = ProfId, User = new User() });

            var handler = new GetProfessionalByIdQueryHandler(_prosRepo);
            var result = await handler.HandleAsync(new GetProfessionalByIdQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(ProfId);
        }

        // ── GetProfessionals (paged) ─────────────────────────────────────────

        [Fact]
        public async Task GetProfessionals_ReturnsMappedPagedResponse()
        {
            _prosRepo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<string?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(), Arg.Any<List<int>?>(),
                Arg.Any<CancellationToken>())
            .Returns(OnePagedPro());

            var handler = new GetProfessionalsQueryHandler(_prosRepo);
            var result = await handler.HandleAsync(
                new GetProfessionalsQuery(1, 10, null, null, null, null, null, "asc"), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
            result.Data.Data.Should().HaveCount(1);
        }

        // ── GetPendingProfessionals ──────────────────────────────────────────

        [Fact]
        public async Task GetPendingProfessionals_GlobalAdmin_PassesNullInstitutionIds()
        {
            _httpCtx.GetCurrentUserId().Returns((Guid?)null);
            _prosRepo.GetPendingPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(), Arg.Any<List<int>?>(),
                Arg.Any<CancellationToken>())
            .Returns(OnePagedPro());

            var handler = new GetPendingProfessionalsQueryHandler(_prosRepo, _adminRepo, _httpCtx);
            var result = await handler.HandleAsync(
                new GetPendingProfessionalsQuery(1, 10, null, null, "asc"), default);

            result.Success.Should().BeTrue();
            await _prosRepo.Received(1).GetPendingPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(),
                Arg.Is<List<int>?>(ids => ids == null),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetPendingProfessionals_InstitutionalAdmin_PassesInstitutionIds()
        {
            _httpCtx.GetCurrentUserId().Returns(AdminId);
            _adminRepo.GetActiveInstitutionIdsByAdminAsync(AdminId, Arg.Any<CancellationToken>())
                      .Returns(new List<int> { 1, 2 });
            _prosRepo.GetPendingPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(), Arg.Any<List<int>?>(),
                Arg.Any<CancellationToken>())
            .Returns(OnePagedPro());

            var handler = new GetPendingProfessionalsQueryHandler(_prosRepo, _adminRepo, _httpCtx);
            await handler.HandleAsync(new GetPendingProfessionalsQuery(1, 10, null, null, "asc"), default);

            await _prosRepo.Received(1).GetPendingPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(),
                Arg.Is<List<int>?>(ids => ids != null && ids.Count == 2),
                Arg.Any<CancellationToken>());
        }

        // ── GetProfessionalStatusHistory ─────────────────────────────────────

        [Fact]
        public async Task GetProfessionalStatusHistory_ReturnsMappedList()
        {
            _prosRepo.GetStatusHistoryAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(new List<ProfessionalStatusHistory>
                     {
                         new() { ProfessionalId = ProfId },
                     });

            var handler = new GetProfessionalStatusHistoryQueryHandler(_prosRepo);
            var result = await handler.HandleAsync(
                new GetProfessionalStatusHistoryQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
        }
    }
}
