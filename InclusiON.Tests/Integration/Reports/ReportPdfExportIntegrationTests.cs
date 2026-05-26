using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;
using InclusiON.Tests.Integration.TestSupport;

namespace InclusiON.Tests.Integration.Reports
{
    /// <summary>
    /// Smoke tests para el endpoint GET /api/reports/{id}/export-pdf.
    /// Verifican que el pipeline HTTP (auth, políticas de permiso, routing) está correctamente configurado.
    /// No validan el contenido del PDF — eso lo cubre la suite unitaria de ReportPdfService.
    /// </summary>
    public class ReportPdfExportIntegrationTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly HttpClient _client;

        public ReportPdfExportIntegrationTests(IntegrationTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ── Autenticación ────────────────────────────────────────────────────

        [Fact]
        public async Task ExportPdf_WithoutAuth_Returns401()
        {
            var response = await _client.GetAsync("/api/reports/1/export-pdf");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── Autorización ─────────────────────────────────────────────────────

        [Fact]
        public async Task ExportPdf_WithFamilyTokenMissingExportPermission_Returns403()
        {
            // Token familiar SIN reports:export — solo tiene reports:read
            var token   = TokenHelper.ForFamilyRepresentative(Guid.NewGuid());
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/1/export-pdf");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                "un familiar sin reports:export debe recibir 403 antes de que el handler se ejecute");
        }

        // ── Política correcta supera el check de permiso ─────────────────────

        [Fact]
        public async Task ExportPdf_WithExportPermission_PassesAuthGate()
        {
            // Token con reports:export — supera el check de política.
            // Sin un reporte real en DB, el handler devuelve 404 (o 400 si el ID no desencripta).
            // Lo que NO debe ocurrir es un 401 ni un 403.
            var token   = TokenHelper.ForFamilyRepresentativeWithExport(Guid.NewGuid());
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/1/export-pdf");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                "un token con reports:export debe superar el check de política");
        }
    }
}
