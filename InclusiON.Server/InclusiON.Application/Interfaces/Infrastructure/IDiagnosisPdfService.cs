using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IDiagnosisPdfService
    {
        byte[] Generate(Diagnosis diagnosis);
    }
}
