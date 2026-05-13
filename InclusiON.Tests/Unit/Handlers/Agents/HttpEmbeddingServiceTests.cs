using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Infrastructure.Services;

namespace InclusiON.Tests.Unit.Handlers.Agents;

public class HttpEmbeddingServiceTests
{
    readonly IHttpClientFactory _factory = Substitute.For<IHttpClientFactory>();

    static readonly float[] SampleVector = [0.1f, 0.2f, 0.3f];

    HttpEmbeddingService BuildSut(HttpClient client)
    {
        _factory.CreateClient("PythonAgent").Returns(client);
        return new HttpEmbeddingService(_factory, NullLogger<HttpEmbeddingService>.Instance);
    }

    static HttpClient MockHttpClient(HttpStatusCode status, object? body = null)
    {
        var handler = new MockHandler(_ =>
        {
            var response = new HttpResponseMessage(status);
            if (body is not null)
                response.Content = new StringContent(JsonSerializer.Serialize(body));
            return response;
        });
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
    }

    [Fact]
    public async Task ReturnsVector_WhenPythonRespondsOk()
    {
        var client = MockHttpClient(HttpStatusCode.OK, new { vector = SampleVector });
        var sut = BuildSut(client);

        var result = await sut.GenerateEmbeddingAsync("test text");

        Assert.Equal(3, result.Length);
        Assert.Equal(0.1f, result[0]);
    }

    [Fact]
    public async Task Throws_WhenPythonReturnsNullVector()
    {
        var client = MockHttpClient(HttpStatusCode.OK, new { vector = (object?)null });
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateEmbeddingAsync("test text"));
    }

    [Fact]
    public async Task Throws_WhenPythonReturnsEmptyVector()
    {
        var client = MockHttpClient(HttpStatusCode.OK, new { vector = Array.Empty<float>() });
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateEmbeddingAsync("test text"));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Throws_WhenPythonReturnsErrorStatusCode(HttpStatusCode statusCode)
    {
        var client = MockHttpClient(statusCode);
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GenerateEmbeddingAsync("test text"));
    }

    [Fact]
    public async Task Throws_WhenHttpThrows()
    {
        var handler = new MockHandler(_ => throw new HttpRequestException("network error"));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
        var sut = BuildSut(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GenerateEmbeddingAsync("test text"));
    }

    sealed class MockHandler : HttpMessageHandler
    {
        readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_handler(request));
    }
}
