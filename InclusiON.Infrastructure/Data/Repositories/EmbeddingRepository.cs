using Npgsql;
using Pgvector;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class EmbeddingRepository : IEmbeddingRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public EmbeddingRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task StoreAsync(int activityId, float[] embedding, CancellationToken cancellationToken = default)
        {
            await using var cmd = _dataSource.CreateCommand(
                "UPDATE \"ActivityEmbeddings\" SET \"Embedding\" = $1 WHERE \"ActivityId\" = $2");
            cmd.Parameters.AddWithValue(new Vector(embedding));
            cmd.Parameters.AddWithValue(activityId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<List<int>> SearchAsync(
            float[] queryEmbedding,
            Guid professionalId,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            // Similitud coseno: 1 - distancia coseno (<=>).
            // Filtra actividades activas que el profesional puede ver:
            //   - propias (ProfessionalId = $2) o estándar (IsStandardActivity = true)
            const string sql = """
                SELECT ae."ActivityId"
                FROM   "ActivityEmbeddings" ae
                JOIN   "Activities" a ON a."Id" = ae."ActivityId"
                WHERE  a."IsActive" = true
                  AND  ae."Embedding" IS NOT NULL
                  AND  (a."ProfessionalId" = $2 OR a."IsStandardActivity" = true)
                ORDER BY ae."Embedding" <=> $1::vector
                LIMIT  $3
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
            cmd.Parameters.AddWithValue(professionalId);
            cmd.Parameters.AddWithValue(limit);

            var ids = new List<int>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetInt32(0));

            return ids;
        }
    }
}
