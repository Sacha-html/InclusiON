using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Activities
{
    public class SearchActivitiesSemanticQueryHandlerTests
    {
        private readonly IEmbeddingService    _embeddingService = Substitute.For<IEmbeddingService>();
        private readonly IEmbeddingRepository _embeddingRepo    = Substitute.For<IEmbeddingRepository>();
        private readonly IActivitiesRepository _activitiesRepo  = Substitute.For<IActivitiesRepository>();
        private readonly IEncryptionService   _encryption       = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId    = Guid.NewGuid();
        private static readonly float[] FakeEmbedding = new float[384];

        public SearchActivitiesSemanticQueryHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private SearchActivitiesSemanticQueryHandler BuildSut() =>
            new(_embeddingService, _embeddingRepo, _activitiesRepo,
                NullLogger<SearchActivitiesSemanticQueryHandler>.Instance,
                _encryption);

        private static SearchActivitiesSemanticQuery AQuery(string text = "actividad motora") =>
            new(ProfId, text, Limit: 10);

        private static Activity AnActivity(int id) => new()
        {
            Id    = id,
            Title = $"Actividad {id}",
        };

        [Fact]
        public async Task NoMatchingEmbeddings_ReturnsEmptySuccessList()
        {
            _embeddingService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                             .Returns(FakeEmbedding);
            _embeddingRepo.SearchAsync(FakeEmbedding, ProfId, 10, Arg.Any<CancellationToken>())
                          .Returns(new List<int>());

            var result = await BuildSut().HandleAsync(AQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
            await _activitiesRepo.DidNotReceive()
                .GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task MatchingEmbeddings_LoadsActivitiesAndMaps()
        {
            var ids = new List<int> { 1, 2 };
            _embeddingService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                             .Returns(FakeEmbedding);
            _embeddingRepo.SearchAsync(FakeEmbedding, ProfId, 10, Arg.Any<CancellationToken>())
                          .Returns(ids);
            _activitiesRepo.GetByIdsAsync(ids, Arg.Any<CancellationToken>())
                           .Returns(new List<Activity> { AnActivity(1), AnActivity(2) });

            var result = await BuildSut().HandleAsync(AQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.Should().HaveCount(2);
            result.Data![0].Title.Should().Be("Actividad 1");
            result.Data![1].Title.Should().Be("Actividad 2");
        }

        [Fact]
        public async Task EmbeddingServiceThrows_ReturnsInternalError()
        {
            _embeddingService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                             .ThrowsAsync(new InvalidOperationException("embedding failed"));

            var result = await BuildSut().HandleAsync(AQuery(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }
    }
}
