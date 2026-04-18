using System.Net;
using FluentAssertions;
using Xunit;
using InclusiON.Tests.Integration.TestSupport;

namespace InclusiON.Tests.Integration.Smoke
{
    /// <summary>
    /// Smoke tests basicos: validan que el pipeline HTTP completo levanta y responde.
    /// No validan logica de negocio — eso va en suites especificas.
    /// Si estos tests fallan, hay un problema estructural (DI, middleware, auth).
    /// </summary>
    public class ApiSmokeTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly HttpClient _client;

        public ApiSmokeTests(IntegrationTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Protected_endpoint_without_auth_returns_401()
        {
            // Prueba que el pipeline (auth middleware, routing, DI) esta bien configurado.
            var response = await _client.GetAsync("/api/persons");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
