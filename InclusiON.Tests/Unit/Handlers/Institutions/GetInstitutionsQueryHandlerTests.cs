using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Handlers;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Institutions
{
    public class GetInstitutionsQueryHandlerTests
    {
        private readonly IInstitutionsRepository _repo = Substitute.For<IInstitutionsRepository>();

        [Fact]
        public async Task HandleAsync_ReturnsAllInstitutions()
        {
            _repo.GetAllAsync(Arg.Any<CancellationToken>())
                 .Returns(new List<EducationalInstitution>
                 {
                     new() { Id = 1, Name = "Escuela A" },
                     new() { Id = 2, Name = "Escuela B" },
                 });

            var handler = new GetInstitutionsQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetInstitutionsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(2);
        }

        [Fact]
        public async Task HandleAsync_EmptyList_ReturnsEmptyData()
        {
            _repo.GetAllAsync(Arg.Any<CancellationToken>())
                 .Returns(new List<EducationalInstitution>());

            var handler = new GetInstitutionsQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetInstitutionsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
