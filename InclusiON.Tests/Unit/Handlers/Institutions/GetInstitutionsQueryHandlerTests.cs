using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Handlers;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Institutions
{
    public class GetInstitutionsQueryHandlerTests
    {
        private readonly IInstitutionsRepository _repo = Substitute.For<IInstitutionsRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        [Fact]
        public async Task HandleAsync_ReturnsAllInstitutions()
        {
            var list = new List<EducationalInstitution>
            {
                new() { Id = 1, Name = "Escuela A" },
                new() { Id = 2, Name = "Escuela B" },
            };
            _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResponse<EducationalInstitution> { Data = list, TotalRecords = list.Count, TotalPages = 1, CurrentPage = 1, PageSize = 10 });

            var handler = new GetInstitutionsQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(new GetInstitutionsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task HandleAsync_EmptyList_ReturnsEmptyData()
        {
            _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResponse<EducationalInstitution>());

            var handler = new GetInstitutionsQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(new GetInstitutionsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
        }
    }
}
