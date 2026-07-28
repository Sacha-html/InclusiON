using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InclusiON.Infrastructure.Services
{
    public class ReportPdfService : IReportPdfService
    {
        // ── Colores institucionales ──────────────────────────────────────
        private static readonly string PrimaryColor   = "#2E5FA3"; // azul InclusiON
        private static readonly string SecondaryColor = "#F5F7FA"; // fondo secciones
        private static readonly string TextMuted      = "#6B7280"; // gris suave

        public byte[] Generate(Report report)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader(report));
                    page.Content().PaddingTop(12).Element(ComposeContent(report));
                    page.Footer().Element(ComposeFooter());
                });
            });

            return doc.GeneratePdf();
        }

        // ── Header ───────────────────────────────────────────────────────

        private static Action<IContainer> ComposeHeader(Report report) => c =>
        {
            c.Column(col =>
            {
                // Título de marca
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("InclusiON")
                        .FontSize(20).Bold().FontColor(PrimaryColor);

                    row.ConstantItem(200).AlignRight()
                        .Text($"Reporte de Progreso")
                        .FontSize(13).Bold().FontColor(PrimaryColor);
                });

                col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(PrimaryColor);

                // Título del reporte
                col.Item().PaddingTop(8).Text(report.Title)
                    .FontSize(15).Bold();

                // Tipo
                col.Item().PaddingTop(2).Text(report.ReportType?.Name ?? "—")
                    .FontSize(10).FontColor(TextMuted);
            });
        };

        // ── Content ──────────────────────────────────────────────────────

        private static Action<IContainer> ComposeContent(Report report) => c =>
        {
            c.Column(col =>
            {
                col.Spacing(10);

                // Tabla de metadatos
                col.Item().Element(MetaTable(report));

                // Secciones de texto
                if (!string.IsNullOrWhiteSpace(report.Content))
                    col.Item().Element(Section("Contenido", report.Content));

                if (!string.IsNullOrWhiteSpace(report.AchievedGoals))
                    col.Item().Element(Section("Metas alcanzadas", report.AchievedGoals));

                if (!string.IsNullOrWhiteSpace(report.AreasToReinforce))
                    col.Item().Element(Section("Áreas a reforzar", report.AreasToReinforce));

                if (!string.IsNullOrWhiteSpace(report.FutureRecommendations))
                    col.Item().Element(Section("Recomendaciones futuras", report.FutureRecommendations));

                if (!string.IsNullOrWhiteSpace(report.NextObjectives))
                    col.Item().Element(Section("Próximos objetivos", report.NextObjectives));

                if (!string.IsNullOrWhiteSpace(report.AdminComment))
                    col.Item().Element(Section("Observación del administrador", report.AdminComment));
            });
        };

        // ── Meta table ───────────────────────────────────────────────────

        private static Action<IContainer> MetaTable(Report report) => c =>
        {
            var personName       = report.Person != null
                ? $"{report.Person.FirstName} {report.Person.LastName}"
                : "—";
            var professionalName = report.Professional != null
                ? $"{report.Professional.FirstName} {report.Professional.LastName}"
                : "—";
            var period = (report.PeriodStartDate.HasValue && report.PeriodEndDate.HasValue)
                ? $"{report.PeriodStartDate.Value:dd/MM/yyyy} — {report.PeriodEndDate.Value:dd/MM/yyyy}"
                : "—";

            c.Background(SecondaryColor).Padding(12).Column(col =>
            {
                col.Spacing(4);

                MetaRow(col, "Fecha del reporte:", report.ReportDate.ToString("dd/MM/yyyy"));
                MetaRow(col, "Período evaluado:", period);
                MetaRow(col, "Persona:",          personName);
                MetaRow(col, "Profesional:",      professionalName);

                if (report.ApprovedAt.HasValue)
                    MetaRow(col, "Aprobado el:", report.ApprovedAt.Value.ToString("dd/MM/yyyy HH:mm") + " UTC");
            });
        };

        private static void MetaRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(130).Text(label).Bold().FontSize(9).FontColor(TextMuted);
                row.RelativeItem().Text(value).FontSize(9);
            });
        }

        // ── Section block ────────────────────────────────────────────────

        private static Action<IContainer> Section(string title, string? body) => c =>
        {
            c.Column(col =>
            {
                col.Item().BorderBottom(1).BorderColor(PrimaryColor)
                    .PaddingBottom(3)
                    .Text(title).Bold().FontSize(11).FontColor(PrimaryColor);

                col.Item().PaddingTop(5).Text(body ?? string.Empty).FontSize(10);
            });
        };

        // ── Footer ───────────────────────────────────────────────────────

        private static Action<IContainer> ComposeFooter() => c =>
        {
            c.Row(row =>
            {
                row.RelativeItem().Text($"Generado el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                    .FontSize(8).FontColor(TextMuted);

                row.ConstantItem(80).AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(8).FontColor(TextMuted);
                    text.CurrentPageNumber().FontSize(8).FontColor(TextMuted);
                    text.Span(" de ").FontSize(8).FontColor(TextMuted);
                    text.TotalPages().FontSize(8).FontColor(TextMuted);
                });
            });
        };
    }
}
