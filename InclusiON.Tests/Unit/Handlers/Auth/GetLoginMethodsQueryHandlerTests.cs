using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Handlers;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Unit.Handlers.Auth
{
    public class GetLoginMethodsQueryHandlerTests
    {
        private readonly IVisualLoginRepository _repo = Substitute.For<IVisualLoginRepository>();
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private GetLoginMethodsQueryHandler BuildSut() =>
            new(_repo, _cache, NullLogger<GetLoginMethodsQueryHandler>.Instance);

        [Fact]
        public async Task CacheMiss_CallsRepositoryAndCachesResult()
        {
            _repo.GetActiveLoginMethodsAsync(Arg.Any<CancellationToken>())
                .Returns(new List<LoginMethod>
                {
                    new() { Id = 1, Name = "Contraseña", Code = "STANDARD" },
                    new() { Id = 2, Name = "PIN", Code = "PIN" }
                });

            var result = await BuildSut().HandleAsync(new GetLoginMethodsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            await _repo.Received(1).GetActiveLoginMethodsAsync(Arg.Any<CancellationToken>());

            // Result is now cached
            _cache.TryGetValue("LoginMethods_Active", out List<LoginMethodResponse>? cached).Should().BeTrue();
            cached.Should().HaveCount(2);
        }

        [Fact]
        public async Task CacheHit_DoesNotCallRepository()
        {
            var cached = new List<LoginMethodResponse>
            {
                new() { Id = 1, Name = "Contraseña" }
            };
            _cache.Set("LoginMethods_Active", cached);

            var result = await BuildSut().HandleAsync(new GetLoginMethodsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            await _repo.DidNotReceive().GetActiveLoginMethodsAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EmptyRepository_ReturnsEmptyList()
        {
            _repo.GetActiveLoginMethodsAsync(Arg.Any<CancellationToken>())
                .Returns(new List<LoginMethod>());

            var result = await BuildSut().HandleAsync(new GetLoginMethodsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
