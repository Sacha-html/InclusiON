namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IEmbeddingRepository
    {
        Task StoreAsync(int activityId, float[] embedding, CancellationToken cancellationToken = default);
        Task StorePersonAsync(Guid personId, float[] embedding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el embedding de una actividad por su ID.
        /// </summary>
        Task<float[]?> GetByActivityIdAsync(int activityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el embedding de una persona por su ID.
        /// </summary>
        Task<float[]?> GetPersonEmbeddingAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Búsqueda por similitud coseno contra los embeddings almacenados.
        /// Devuelve los IDs de actividad ordenados de mayor a menor similitud,
        /// filtrando solo actividades activas accesibles al profesional indicado
        /// (propias + estándar).
        /// </summary>
        Task<List<int>> SearchAsync(
            float[] queryEmbedding,
            Guid professionalId,
            int limit = 10,
            List<int>? excludeIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Búsqueda de actividades recomendadas para una persona por similitud coseno
        /// entre el embedding del perfil de la persona y los embeddings de actividades.
        /// </summary>
        Task<List<int>> SearchActivitiesForPersonAsync(
            Guid personId,
            Guid professionalId,
            int limit = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Búsqueda de personas compatibles con una actividad por similitud coseno
        /// entre el embedding de la actividad y los embeddings de personas.
        /// </summary>
        Task<List<Guid>> SearchPersonsForActivityAsync(
            int activityId,
            Guid professionalId,
            int limit = 10,
            CancellationToken cancellationToken = default);
    }
}
