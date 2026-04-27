using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Xunit;
using InclusiON.Tests.Integration.TestSupport;

namespace InclusiON.Tests.Integration.Authorization
{
    /// <summary>
    /// Tests de integración para autorización por recurso (HU-IN-172 — CA-15).
    /// Valida la respuesta HTTP de los 5 endpoints críticos ante distintos roles.
    ///
    /// Convenciones de aserción:
    ///   • Casos denegados  → se verifica el código exacto (403 o 404 según CA-17).
    ///   • Casos permitidos → se verifica que la respuesta NO sea 401 ni 403,
    ///     es decir, que el check de autorización por recurso haya pasado.
    /// </summary>
    public class ResourceAuthorizationIntegrationTests : IClassFixture<AuthorizationTestFixture>
    {
        private readonly AuthorizationTestFixture _fixture;
        private readonly HttpClient _client;

        public ResourceAuthorizationIntegrationTests(AuthorizationTestFixture fixture)
        {
            _fixture = fixture;
            _client  = fixture.CreateClient();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET /api/persons/{id}
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetPerson_Professional_Assigned_IsAllowed()
        {
            var token = TokenHelper.ForProfessional(_fixture.AssignedProfessionalUserId);
            var response = await GetAsync($"/api/persons/{_fixture.PersonId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPerson_Professional_Unassigned_Returns403()
        {
            var token = TokenHelper.ForProfessional(_fixture.UnassignedProfessionalUserId);
            var response = await GetAsync($"/api/persons/{_fixture.PersonId}", token);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPerson_Family_WithActiveLink_IsAllowed()
        {
            var token = TokenHelper.ForFamilyRepresentative(_fixture.FamilyWithLinkUserId);
            var response = await GetAsync($"/api/persons/{_fixture.PersonId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPerson_Family_WithoutLink_Returns404()
        {
            var token = TokenHelper.ForFamilyRepresentative(_fixture.FamilyWithoutLinkUserId);
            var response = await GetAsync($"/api/persons/{_fixture.PersonId}", token);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPerson_GlobalAdmin_IsAllowed()
        {
            var token = TokenHelper.ForGlobalAdmin(_fixture.GlobalAdminUserId);
            var response = await GetAsync($"/api/persons/{_fixture.PersonId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET /api/diagnoses/{id}
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetDiagnosis_Professional_Assigned_IsAllowed()
        {
            var token = TokenHelper.ForProfessional(_fixture.AssignedProfessionalUserId);
            var response = await GetAsync($"/api/diagnoses/{_fixture.DiagnosisId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDiagnosis_Professional_Unassigned_Returns403()
        {
            var token = TokenHelper.ForProfessional(_fixture.UnassignedProfessionalUserId);
            var response = await GetAsync($"/api/diagnoses/{_fixture.DiagnosisId}", token);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDiagnosis_GlobalAdmin_IsAllowed()
        {
            var token = TokenHelper.ForGlobalAdmin(_fixture.GlobalAdminUserId);
            var response = await GetAsync($"/api/diagnoses/{_fixture.DiagnosisId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET /api/reports/{id}
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetReport_Professional_Assigned_IsAllowed()
        {
            var token = TokenHelper.ForProfessional(_fixture.AssignedProfessionalUserId);
            var response = await GetAsync($"/api/reports/{_fixture.ReportId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetReport_Professional_Unassigned_Returns403()
        {
            var token = TokenHelper.ForProfessional(_fixture.UnassignedProfessionalUserId);
            var response = await GetAsync($"/api/reports/{_fixture.ReportId}", token);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetReport_Family_WithActiveLink_IsAllowed()
        {
            var token = TokenHelper.ForFamilyRepresentative(_fixture.FamilyWithLinkUserId);
            var response = await GetAsync($"/api/reports/{_fixture.ReportId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReport_Family_WithoutLink_Returns404()
        {
            var token = TokenHelper.ForFamilyRepresentative(_fixture.FamilyWithoutLinkUserId);
            var response = await GetAsync($"/api/reports/{_fixture.ReportId}", token);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReport_GlobalAdmin_IsAllowed()
        {
            var token = TokenHelper.ForGlobalAdmin(_fixture.GlobalAdminUserId);
            var response = await GetAsync($"/api/reports/{_fixture.ReportId}", token);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PUT /api/persons/{id}
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task UpdatePerson_Professional_Assigned_IsAllowed()
        {
            var token = TokenHelper.ForProfessional(_fixture.AssignedProfessionalUserId);
            var response = await PutAsync($"/api/persons/{_fixture.PersonId}", token, "{}");

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdatePerson_Professional_Unassigned_Returns403()
        {
            var token = TokenHelper.ForProfessional(_fixture.UnassignedProfessionalUserId);
            var response = await PutAsync($"/api/persons/{_fixture.PersonId}", token, "{}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdatePerson_GlobalAdmin_IsAllowed()
        {
            var token = TokenHelper.ForGlobalAdmin(_fixture.GlobalAdminUserId);
            var response = await PutAsync($"/api/persons/{_fixture.PersonId}", token, "{}");

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // POST /api/persons/{personId}/diagnoses
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task CreateDiagnosis_Professional_Assigned_IsAllowed()
        {
            var token = TokenHelper.ForProfessional(_fixture.AssignedProfessionalUserId);
            var body = """
                {
                  "diagnosisDate": "2026-03-01",
                  "primaryDiagnosis": "Diagnóstico de prueba"
                }
                """;
            var response = await PostAsync($"/api/persons/{_fixture.PersonId}/diagnoses", token, body);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateDiagnosis_Professional_Unassigned_Returns403()
        {
            var token = TokenHelper.ForProfessional(_fixture.UnassignedProfessionalUserId);
            var body = """
                {
                  "diagnosisDate": "2026-03-01",
                  "primaryDiagnosis": "Diagnóstico de prueba"
                }
                """;
            var response = await PostAsync($"/api/persons/{_fixture.PersonId}/diagnoses", token, body);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateDiagnosis_GlobalAdmin_IsAllowed()
        {
            var token = TokenHelper.ForGlobalAdmin(_fixture.GlobalAdminUserId);
            var body = """
                {
                  "diagnosisDate": "2026-03-01",
                  "primaryDiagnosis": "Diagnóstico de prueba admin"
                }
                """;
            var response = await PostAsync($"/api/persons/{_fixture.PersonId}/diagnoses", token, body);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Sin autenticación → 401 en todos los endpoints protegidos
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetPerson_NoAuth_Returns401()
        {
            var response = await _client.GetAsync($"/api/persons/{_fixture.PersonId}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDiagnosis_NoAuth_Returns401()
        {
            var response = await _client.GetAsync($"/api/diagnoses/{_fixture.DiagnosisId}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetReport_NoAuth_Returns401()
        {
            var response = await _client.GetAsync($"/api/reports/{_fixture.ReportId}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════════════════

        private Task<HttpResponseMessage> GetAsync(string url, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _client.SendAsync(request);
        }

        private Task<HttpResponseMessage> PutAsync(string url, string token, string json)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _client.SendAsync(request);
        }

        private Task<HttpResponseMessage> PostAsync(string url, string token, string json)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _client.SendAsync(request);
        }
    }
}
