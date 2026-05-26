using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IReportPdfService
    {
        byte[] Generate(Report report);
    }
}
