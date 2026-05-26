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

        public async Task StorePersonAsync(Guid personId, float[] embedding, CancellationToken cancellationToken = default)
        {
            await using var cmd = _dataSource.CreateCommand(
                "UPDATE \"PersonEmbeddings\" SET \"Embedding\" = $1 WHERE \"PersonId\" = $2");
            cmd.Parameters.AddWithValue(new Vector(embedding));
            cmd.Parameters.AddWithValue(personId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<float[]?> GetByActivityIdAsync(int activityId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT "Embedding" FROM "ActivityEmbeddings" WHERE "ActivityId" = $1
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(activityId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken) && !reader.IsDBNull(0))
            {
                var vector = (Vector)reader.GetValue(0);
                return vector.ToArray();
            }

            return null;
        }

        public async Task<float[]?> GetPersonEmbeddingAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT "Embedding" FROM "PersonEmbeddings" WHERE "PersonId" = $1
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(personId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken) && !reader.IsDBNull(0))
            {
                var vector = (Vector)reader.GetValue(0);
                return vector.ToArray();
            }

            return null;
        }

        public async Task<List<int>> SearchAsync(
            float[] queryEmbedding,
            Guid professionalId,
            int limit = 10,
            List<int>? excludeIds = null,
            float minSimilarity = 0.25f,
            CancellationToken cancellationToken = default)
        {
            // Similitud coseno: 1 - distancia coseno (<=>).
            // Filtra actividades activas que el profesional puede ver:
            //   - propias (ProfessionalId = $2) o estándar (IsStandardActivity = true)
            // $4 = minSimilarity threshold (similitud coseno mínima requerida)
            var sql = """
                SELECT ae."ActivityId"
                FROM   "ActivityEmbeddings" ae
                JOIN   "Activities" a ON a."Id" = ae."ActivityId"
                WHERE  a."IsActive" = true
                  AND  ae."Embedding" IS NOT NULL
                  AND  (a."ProfessionalId" = $2 OR a."IsStandardActivity" = true)
                  AND  (1 - (ae."Embedding" <=> $1::vector)) >= $4
                """;

            if (excludeIds is { Count: > 0 })
            {
                sql += $" AND ae.\"ActivityId\" NOT IN ({string.Join(",", excludeIds)})";
            }

            sql += """

                ORDER BY ae."Embedding" <=> $1::vector
                LIMIT  $3
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
            cmd.Parameters.AddWithValue(professionalId);
            cmd.Parameters.AddWithValue(limit);
            cmd.Parameters.AddWithValue(minSimilarity);

            var ids = new List<int>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public async Task<List<int>> SearchActivitiesForPersonAsync(
            Guid personId,
            Guid professionalId,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT ae."ActivityId"
                FROM   "ActivityEmbeddings" ae
                JOIN   "Activities" a ON a."Id" = ae."ActivityId"
                JOIN   "PersonEmbeddings" pe ON pe."PersonId" = $1
                WHERE  a."IsActive" = true
                  AND  ae."Embedding" IS NOT NULL
                  AND  pe."Embedding" IS NOT NULL
                  AND  (a."ProfessionalId" = $2 OR a."IsStandardActivity" = true)
                ORDER BY ae."Embedding" <=> pe."Embedding"
                LIMIT  $3
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(personId);
            cmd.Parameters.AddWithValue(professionalId);
            cmd.Parameters.AddWithValue(limit);

            var ids = new List<int>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public async Task<List<Guid>> SearchPersonsForActivityAsync(
            int activityId,
            Guid professionalId,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT pe."PersonId"
                FROM   "PersonEmbeddings" pe
                JOIN   "PersonProfessionalAssignments" ppa ON ppa."PersonId" = pe."PersonId"
                JOIN   "ActivityEmbeddings" ae ON ae."ActivityId" = $1
                WHERE  pe."Embedding" IS NOT NULL
                  AND  ae."Embedding" IS NOT NULL
                  AND  ppa."ProfessionalId" = $2
                  AND  ppa."IsActive" = true
                ORDER BY pe."Embedding" <=> ae."Embedding"
                LIMIT  $3
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(activityId);
            cmd.Parameters.AddWithValue(professionalId);
            cmd.Parameters.AddWithValue(limit);

            var ids = new List<Guid>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetGuid(0));

            return ids;
        }
    }
}
