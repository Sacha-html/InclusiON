using Npgsql;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using System.Data;

namespace InclusiON.Infrastructure.Data
{
    /// <summary>
    /// Implementacion de IRawDbExecutor usando ADO.NET puro (Npgsql).
    ///
    /// Cada operacion de lectura abre y cierra su propia conexion (connection-per-call).
    /// Las operaciones transaccionales manejan conexion + transaccion como una unidad.
    ///
    /// NOTA: Si en el futuro se instala Dapper (dotnet add package Dapper),
    /// se pueden reemplazar los metodos internos con:
    ///   connection.QueryAsync&lt;T&gt;(sql, parameters)        — para queries
    ///   connection.ExecuteAsync(sql, parameters)             — para non-query
    ///   connection.ExecuteScalarAsync&lt;T&gt;(sql, parameters) — para scalars
    /// Dapper simplifica el mapeo pero la interfaz publica no cambia.
    /// </summary>
    public class RawDbExecutor : IRawDbExecutor
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RawDbExecutor> _logger;

        public RawDbExecutor(IConnectionFactory connectionFactory, ILogger<RawDbExecutor> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────
        // QUERIES (SELECT)
        // ──────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<T>> QueryAsync<T>(
            string sql,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();

            using var command = new NpgsqlCommand(sql, connection);
            configureParams?.Invoke(command);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var results = new List<T>();
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(mapper(reader));
            }

            return results;
        }

        public async Task<T?> QuerySingleAsync<T>(
            string sql,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();

            using var command = new NpgsqlCommand(sql, connection);
            configureParams?.Invoke(command);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return mapper(reader);
            }

            return default;
        }

        public async Task<T?> ExecuteScalarAsync<T>(
            string sql,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();

            using var command = new NpgsqlCommand(sql, connection);
            configureParams?.Invoke(command);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is null || result == DBNull.Value)
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        // ──────────────────────────────────────────────────────────────
        // STORED PROCEDURES
        // ──────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<T>> ExecuteSpAsync<T>(
            string storedProcedure,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();

            using var command = new NpgsqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            configureParams?.Invoke(command);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var results = new List<T>();
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(mapper(reader));
            }

            return results;
        }

        public async Task<int> ExecuteSpNonQueryAsync(
            string storedProcedure,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();

            using var command = new NpgsqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            configureParams?.Invoke(command);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        // ──────────────────────────────────────────────────────────────
        // TRANSACCIONES ADO.NET
        // ──────────────────────────────────────────────────────────────

        public async Task ExecuteInTransactionAsync(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await operation(connection, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET transaction failed, rolling back");
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback also failed");
                }
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            await using var connection = (NpgsqlConnection)await _connectionFactory.CreateConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation(connection, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET transaction failed, rolling back");
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback also failed");
                }
                throw;
            }
        }
    }
}
