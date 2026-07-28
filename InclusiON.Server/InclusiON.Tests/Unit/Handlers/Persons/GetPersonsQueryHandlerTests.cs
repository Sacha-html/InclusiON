using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class GetPersonsQueryHandlerTests
    {
        private readonly IPersonsRepository _repo = Substitute.For<IPersonsRepository>();

        private static readonly Guid PersonId = Guid.NewGuid();

        private static PersonWithDisability APerson() => new()
        {
            Id = PersonId, User = new User { IsActive = true },
            BirthDate = new DateTime(2000, 1, 1),
        };

        // ── GetPersonById ────────────────────────────────────────────────────

        [Fact]
        public async Task GetPersonById_NotFound_ReturnsPersonNotFound()
        {
            _repo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                 .Returns((PersonWithDisability?)null);

            var handler = new GetPersonByIdQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetPersonByIdQuery(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task GetPersonById_Found_ReturnsPersonId()
        {
            _repo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                 .Returns(APerson());

            var handler = new GetPersonByIdQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetPersonByIdQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(PersonId);
        }

        // ── GetPersons (paged) ───────────────────────────────────────────────

        [Fact]
        public async Task GetPersons_ReturnsMappedPagedResponse()
        {
            _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(),
                Arg.Any<List<int>?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<Guid>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<PersonWithDisability>
            {
                Data = new List<PersonWithDisability> { APerson() },
                TotalRecords = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10,
            });

            var handler = new GetPersonsQueryHandler(_repo);
            var result = await handler.HandleAsync(
                new GetPersonsQuery(1, 10, null, null, null, null, null, "asc"), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
            result.Data.Data.Should().HaveCount(1);
        }
    }
}
