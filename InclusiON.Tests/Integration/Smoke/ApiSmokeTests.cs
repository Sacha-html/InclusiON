using System.Net;
using System.Net.Http.Headers;
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

        [Fact]
        public async Task Protected_endpoint_with_valid_token_is_not_rejected_as_unauthorized()
        {
            // Verifica que un token firmado con la clave de test sea aceptado por el middleware JWT.
            // Si esto falla, hay un problema de configuracion JWT en IntegrationTestFactory.
            var token = TokenHelper.ForProfessional(Guid.NewGuid());
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/persons");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "un token valido firmado con la clave de test no debe ser rechazado por el middleware JWT");
        }
    }
}
