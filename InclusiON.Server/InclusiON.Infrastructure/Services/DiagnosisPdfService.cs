using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Genera el PDF del diagnóstico en el formato institucional escolar
    /// (cabecera de datos, cajas con borde para Fortalezas/Debilidades/Observaciones
    /// y línea de firma), para su impresión y firma física.
    /// </summary>
    public class DiagnosisPdfService : IDiagnosisPdfService
    {
        private static readonly string PrimaryColor = "#2E5FA3";
        private static readonly string BorderColor  = "#D0D5DD";
        private static readonly string TextMuted    = "#6B7280";

        public byte[] Generate(Diagnosis diagnosis)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader());
                    page.Content().PaddingTop(12).Element(ComposeContent(diagnosis));
                    page.Footer().Element(ComposeFooter(diagnosis));
                });
            });

            return doc.GeneratePdf();
        }

        private static Action<IContainer> ComposeHeader() => c =>
        {
            c.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("InclusiON")
                        .FontSize(20).Bold().FontColor(PrimaryColor);

                    row.ConstantItem(220).AlignRight()
                        .Text("FORMATO DE DIAGNÓSTICO INSTITUCIONAL")
                        .FontSize(12).Bold().FontColor(PrimaryColor);
                });

                col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(PrimaryColor);
            });
        };

        private static Action<IContainer> ComposeContent(Diagnosis d) => c =>
        {
            var studentName = d.Person != null ? $"{d.Person.FirstName} {d.Person.LastName}" : "—";
            var teacherName = d.Professional != null ? $"{d.Professional.FirstName} {d.Professional.LastName}" : "—";

            c.Column(col =>
            {
                col.Spacing(12);

                col.Item().Border(1).BorderColor(BorderColor).Padding(10).Column(meta =>
                {
                    meta.Spacing(4);
                    MetaRow(meta, "Docente:", teacherName, "Asignatura / Área:", d.PrimaryDiagnosis);
                    MetaRow(meta, "Fecha:", d.DiagnosisDate.ToString("dd/MM/yyyy"), "Alumno:", studentName);
                });

                col.Item().Element(Section("FORTALEZAS DEL ESTUDIANTE", d.IdentifiedCapabilities));
                col.Item().Element(Section("DEBILIDADES / DESAFÍOS DEL ESTUDIANTE", d.IdentifiedChallenges));

                if (!string.IsNullOrWhiteSpace(d.InitialObservations))
                    col.Item().Element(Section("OBSERVACIONES ADICIONALES", d.InitialObservations));

                var hasDua = !string.IsNullOrWhiteSpace(d.RequiredSupports) || !string.IsNullOrWhiteSpace(d.RecommendedStrategies);
                if (hasDua)
                {
                    col.Item().PaddingTop(4).Text("APOYOS Y ESTRATEGIAS DUA")
                        .FontSize(11).Bold().FontColor(PrimaryColor);

                    if (!string.IsNullOrWhiteSpace(d.RequiredSupports))
                        col.Item().Element(Section("Apoyos requeridos", d.RequiredSupports));

                    if (!string.IsNullOrWhiteSpace(d.RecommendedStrategies))
                        col.Item().Element(Section("Estrategias recomendadas", d.RecommendedStrategies));
                }

                if (!string.IsNullOrWhiteSpace(d.PedagogicalObjectives))
                    col.Item().Element(Section("Objetivos pedagógicos", d.PedagogicalObjectives));
            });
        };

        private static void MetaRow(ColumnDescriptor col, string label1, string value1, string label2, string value2)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Row(inner =>
                {
                    inner.ConstantItem(90).Text(label1).Bold().FontSize(9).FontColor(TextMuted);
                    inner.RelativeItem().Text(value1).FontSize(9);
                });
                row.RelativeItem().Row(inner =>
                {
                    inner.ConstantItem(90).Text(label2).Bold().FontSize(9).FontColor(TextMuted);
                    inner.RelativeItem().Text(value2).FontSize(9);
                });
            });
        }

        private static Action<IContainer> Section(string title, string? body) => c =>
        {
            c.Column(col =>
            {
                col.Item().Text(title).Bold().FontSize(10).FontColor(PrimaryColor);
                col.Item().PaddingTop(3).Border(1).BorderColor(BorderColor).Padding(8)
                    .MinHeight(40)
                    .Text(body ?? string.Empty).FontSize(10);
            });
        };

        private static Action<IContainer> ComposeFooter(Diagnosis d) => c =>
        {
            var teacherName = d.Professional != null ? $"{d.Professional.FirstName} {d.Professional.LastName}" : "—";

            c.Column(col =>
            {
                col.Item().PaddingTop(20).Row(row =>
                {
                    row.ConstantItem(220).Column(sig =>
                    {
                        sig.Item().LineHorizontal(1).LineColor(BorderColor);
                        sig.Item().PaddingTop(2).Text(teacherName).FontSize(9);
                        sig.Item().Text("Firma y aclaración del Docente").FontSize(8).FontColor(TextMuted);
                    });
                });

                col.Item().PaddingTop(10).Row(row =>
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
            });
        };
    }
}
