namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
