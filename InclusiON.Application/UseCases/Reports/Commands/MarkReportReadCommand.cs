namespace InclusiON.Application.UseCases.Reports.Commands
{
    /// <summary>
    /// Marca un reporte aprobado como leído por el familiar.
    /// El indicador "Nuevo" desaparece en la UI una vez ejecutado.
    /// </summary>
    public record MarkReportReadCommand(int ReportId);
}
