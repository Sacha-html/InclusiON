using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Agents;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Agents;

public class EmbeddingAgentTests
{
    readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    readonly IEmbeddingRepository _embeddingRepo = Substitute.For<IEmbeddingRepository>();
    readonly ILogger<EmbeddingAgent> _logger = NullLogger<EmbeddingAgent>.Instance;

    static readonly float[] SampleVector = [0.1f, 0.2f, 0.3f];

    EmbeddingAgent BuildSut(HttpClient client)
    {
        _httpClientFactory.CreateClient("PythonAgent").Returns(client);
        return new EmbeddingAgent(_httpClientFactory, _embeddingRepo, _logger);
    }

    static HttpClient MockHttpClient(object responseBody)
    {
        var json = JsonSerializer.Serialize(responseBody);
        var handler = new MockHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
    }

    static BackgroundJob CreateJob() => new()
    {
        Id = 1,
        JobTypeId = JobTypes.Embedding,
        StatusId = BackgroundJobStatuses.Running,
        Payload = """{"entity_type":"activity","entity_id":"42","title":"Test","description":"Desc","instructions":"Instr"}""",
        RetryCount = 0,
        MaxRetries = 3
    };

    [Fact]
    public async Task CallsPythonAndStoresEmbedding()
    {
        var client = MockHttpClient(new { vector = SampleVector });
        var sut = BuildSut(client);

        await sut.HandleAsync(CreateJob(), default);

        await _embeddingRepo.Received(1).StoreAsync(42, Arg.Is<float[]>(v => v.Length == 3), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_WhenPythonReturnsEmptyVector()
    {
        var client = MockHttpClient(new { vector = Array.Empty<float>() });
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(CreateJob(), default));
    }

    [Fact]
    public async Task Throws_WhenPythonReturnsNullVector()
    {
        var client = MockHttpClient(new { vector = (object?)null });
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(CreateJob(), default));
    }

    [Fact]
    public async Task PersonEntityType_CallsStorePersonAsync()
    {
        var personId = Guid.NewGuid();
        var job = new BackgroundJob
        {
            Id = 2,
            JobTypeId = JobTypes.Embedding,
            StatusId = BackgroundJobStatuses.Running,
            Payload = JsonSerializer.Serialize(new
            {
                entity_type = "person",
                entity_id = personId.ToString(),
                title = "Test Person",
                description = "Desc"
            }),
            RetryCount = 0,
            MaxRetries = 3
        };
        var client = MockHttpClient(new { vector = SampleVector });
        var sut = BuildSut(client);

        await sut.HandleAsync(job, default);

        await _embeddingRepo.Received(1).StorePersonAsync(personId, Arg.Is<float[]>(v => v.Length == 3), Arg.Any<CancellationToken>());
        await _embeddingRepo.DidNotReceive().StoreAsync(Arg.Any<int>(), Arg.Any<float[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_WhenEntityTypeIsUnknown()
    {
        var job = new BackgroundJob
        {
            Id = 3,
            JobTypeId = JobTypes.Embedding,
            StatusId = BackgroundJobStatuses.Running,
            Payload = """{"entity_type":"unknown","entity_id":"99","title":"X"}""",
            RetryCount = 0,
            MaxRetries = 3
        };
        var client = MockHttpClient(new { vector = SampleVector });
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(job, default));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Throws_WhenPythonReturnsErrorStatusCode(HttpStatusCode statusCode)
    {
        var handler = new MockHandler(_ => new HttpResponseMessage(statusCode));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.HandleAsync(CreateJob(), default));
    }

    sealed class MockHandler : HttpMessageHandler
    {
        readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_handler(request));
    }
}
