using Npgsql;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories;

public class BackgroundJobRepository : IBackgroundJobRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public BackgroundJobRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<BackgroundJob> CreateAsync(int jobTypeId, string payload, DateTime? scheduledAt = null, int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO "BackgroundJobs"
                ("JobTypeId", "StatusId", "Payload", "RetryCount", "MaxRetries", "ScheduledAt", "CreatedAt", "CreatedBy", "UpdatedAt", "IsActive")
            VALUES ($1, $2, $3::jsonb, 0, $4, $5, NOW(), $6, NOW(), true)
            RETURNING *;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(jobTypeId);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Pending);
        cmd.Parameters.AddWithValue(payload);
        cmd.Parameters.AddWithValue(maxRetries);
        cmd.Parameters.AddWithValue(scheduledAt ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue(Guid.Empty);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapJob(reader);
    }

    public async Task<BackgroundJob?> TryClaimAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "BackgroundJobs"
            SET "StatusId" = $1,
                "RetryCount" = "RetryCount" + 1,
                "UpdatedAt" = NOW()
            WHERE "Id" = (
                SELECT "Id" FROM "BackgroundJobs"
                WHERE "StatusId" = $2
                  AND ("ScheduledAt" IS NULL OR "ScheduledAt" <= NOW())
                ORDER BY "CreatedAt" ASC
                LIMIT 1
                FOR UPDATE SKIP LOCKED
            )
            RETURNING *;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Running);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Pending);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            return MapJob(reader);

        return null;
    }

    public async Task CompleteAsync(int jobId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "BackgroundJobs"
            SET "StatusId" = $1, "CompletedAt" = NOW(), "UpdatedAt" = NOW()
            WHERE "Id" = $2;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Completed);
        cmd.Parameters.AddWithValue(jobId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task FailAsync(int jobId, string errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "BackgroundJobs"
            SET "StatusId" = $1, "ErrorMessage" = $2, "UpdatedAt" = NOW()
            WHERE "Id" = $3;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Failed);
        cmd.Parameters.AddWithValue(errorMessage);
        cmd.Parameters.AddWithValue(jobId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RetryAsync(int jobId, string errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "BackgroundJobs"
            SET "StatusId" = $1, "ErrorMessage" = $2, "UpdatedAt" = NOW()
            WHERE "Id" = $3;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Pending);
        cmd.Parameters.AddWithValue(errorMessage);
        cmd.Parameters.AddWithValue(jobId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<BackgroundJob>> GetPendingAsync(int batchSize, DateTime orphanTimeout, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM "BackgroundJobs"
            WHERE ("StatusId" = $1 AND ("ScheduledAt" IS NULL OR "ScheduledAt" <= NOW()))
               OR ("StatusId" = $2 AND "UpdatedAt" < $3)
            ORDER BY "CreatedAt" ASC
            LIMIT $4;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Pending);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Running);
        cmd.Parameters.AddWithValue(orphanTimeout);
        cmd.Parameters.AddWithValue(batchSize);

        var jobs = new List<BackgroundJob>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            jobs.Add(MapJob(reader));

        return jobs;
    }

    public async Task DeleteCompletedOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM "BackgroundJobs"
            WHERE "StatusId" = $1 AND "UpdatedAt" < $2;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Completed);
        cmd.Parameters.AddWithValue(cutoff);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> ResetOrphanedRunningAsync(DateTime orphanTimeout, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "BackgroundJobs"
            SET "StatusId" = $1, "UpdatedAt" = NOW()
            WHERE "StatusId" = $2 AND "UpdatedAt" < $3;
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Pending);
        cmd.Parameters.AddWithValue(BackgroundJobStatuses.Running);
        cmd.Parameters.AddWithValue(orphanTimeout);
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static BackgroundJob MapJob(NpgsqlDataReader reader)
    {
        return new BackgroundJob
        {
            Id = reader.GetInt32(0),
            JobTypeId = reader.GetInt32(1),
            StatusId = reader.GetInt32(2),
            Payload = reader.GetString(3),
            RetryCount = reader.GetInt32(4),
            MaxRetries = reader.GetInt32(5),
            ErrorMessage = reader.IsDBNull(6) ? null : reader.GetString(6),
            ScheduledAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            CompletedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            CreatedAt = reader.GetDateTime(9),
            CreatedBy = reader.GetGuid(10),
            UpdatedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
            UpdatedBy = reader.IsDBNull(12) ? null : reader.GetGuid(12),
            IsActive = reader.GetBoolean(13)
        };
    }
}
