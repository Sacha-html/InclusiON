using System.Data;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Ejecutor de operaciones ADO.NET directas: stored procedures, vistas y queries raw.
    ///
    /// Usar cuando EF Core no es adecuado:
    /// - Stored procedures con logica compleja del lado del servidor
    /// - Vistas SQL que no mapean a entidades del dominio
    /// - Queries de reporting o bulk operations
    /// - Cualquier operacion que necesite control fino sobre la conexion/transaccion
    ///
    /// Para operaciones CRUD normales, seguir usando los repositorios con EF Core.
    /// </summary>
    public interface IRawDbExecutor
    {
        // ──────────────────────────────────────────────────────────────
        // QUERIES (SELECT) — Sin transaccion, conexion propia
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta un query o vista y mapea los resultados con un delegate.
        /// Abre y cierra su propia conexion.
        ///
        /// <example>
        /// Ejemplo — leer una vista:
        /// <code>
        /// var stats = await _db.QueryAsync("SELECT * FROM vw_DashboardStats WHERE Year = @Year",
        ///     reader => new DashboardStat
        ///     {
        ///         Category = reader.GetString(reader.GetOrdinal("Category")),
        ///         Total = reader.GetInt32(reader.GetOrdinal("Total"))
        ///     },
        ///     cmd => cmd.Parameters.AddWithValue("@Year", 2026));
        /// </code>
        /// </example>
        /// </summary>
        Task<IReadOnlyList<T>> QueryAsync<T>(
            string sql,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta un query y retorna un unico resultado o default.
        ///
        /// <example>
        /// Ejemplo — scalar desde vista:
        /// <code>
        /// var count = await _db.QuerySingleAsync&lt;int?&gt;(
        ///     "SELECT TotalActive FROM vw_UserCounts WHERE Role = @Role",
        ///     reader => reader.GetInt32(0),
        ///     cmd => cmd.Parameters.AddWithValue("@Role", "Professional"));
        /// </code>
        /// </example>
        /// </summary>
        Task<T?> QuerySingleAsync<T>(
            string sql,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta un scalar (COUNT, SUM, EXISTS, etc).
        ///
        /// <example>
        /// <code>
        /// var total = await _db.ExecuteScalarAsync&lt;int&gt;(
        ///     "SELECT COUNT(*) FROM PersonsWithDisabilities WHERE IsActive = 1");
        /// </code>
        /// </example>
        /// </summary>
        Task<T?> ExecuteScalarAsync<T>(
            string sql,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default);

        // ──────────────────────────────────────────────────────────────
        // STORED PROCEDURES — Sin transaccion
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta un stored procedure que retorna filas.
        ///
        /// <example>
        /// Ejemplo — SP con parametros:
        /// <code>
        /// var report = await _db.ExecuteSpAsync("sp_GetPersonReport",
        ///     reader => new PersonReport
        ///     {
        ///         PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
        ///         FullName = reader.GetString(reader.GetOrdinal("FullName")),
        ///         Score = reader.GetDecimal(reader.GetOrdinal("Score"))
        ///     },
        ///     cmd =>
        ///     {
        ///         cmd.Parameters.AddWithValue("@StartDate", startDate);
        ///         cmd.Parameters.AddWithValue("@EndDate", endDate);
        ///     });
        /// </code>
        /// </example>
        /// </summary>
        Task<IReadOnlyList<T>> ExecuteSpAsync<T>(
            string storedProcedure,
            Func<IDataReader, T> mapper,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta un stored procedure que no retorna filas (INSERT, UPDATE, DELETE internos).
        /// Retorna el numero de filas afectadas.
        ///
        /// <example>
        /// <code>
        /// var affected = await _db.ExecuteSpNonQueryAsync("sp_DeactivateExpiredSessions",
        ///     cmd => cmd.Parameters.AddWithValue("@CutoffDate", DateTime.UtcNow.AddDays(-30)));
        /// </code>
        /// </example>
        /// </summary>
        Task<int> ExecuteSpNonQueryAsync(
            string storedProcedure,
            Action<IDbCommand>? configureParams = null,
            CancellationToken cancellationToken = default);

        // ──────────────────────────────────────────────────────────────
        // OPERACIONES CON TRANSACCION ADO.NET
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta multiples operaciones ADO.NET dentro de una transaccion.
        /// La conexion y transaccion se crean automaticamente y se pasan al delegate.
        /// Commit automatico si no hay excepcion; rollback automatico si falla.
        ///
        /// IMPORTANTE: Esta transaccion es independiente de EF Core.
        /// No mezclar con operaciones de DbContext/repositorios EF en el mismo scope.
        /// Para transacciones que mezclen EF + ADO.NET, usar UnitOfWork.ExecuteInTransactionAsync
        /// y acceder a la conexion del DbContext.
        ///
        /// <example>
        /// Ejemplo — multiples SPs en una transaccion:
        /// <code>
        /// await _db.ExecuteInTransactionAsync(async (connection, transaction, ct) =>
        /// {
        ///     // Primer SP
        ///     using var cmd1 = connection.CreateCommand();
        ///     cmd1.Transaction = transaction;
        ///     cmd1.CommandText = "sp_TransferCredits";
        ///     cmd1.CommandType = CommandType.StoredProcedure;
        ///     cmd1.Parameters.AddWithValue("@FromUserId", fromId);
        ///     cmd1.Parameters.AddWithValue("@ToUserId", toId);
        ///     cmd1.Parameters.AddWithValue("@Amount", amount);
        ///     await cmd1.ExecuteNonQueryAsync(ct);
        ///
        ///     // Segundo SP — solo se ejecuta si el primero fue exitoso
        ///     using var cmd2 = connection.CreateCommand();
        ///     cmd2.Transaction = transaction;
        ///     cmd2.CommandText = "sp_LogTransfer";
        ///     cmd2.CommandType = CommandType.StoredProcedure;
        ///     cmd2.Parameters.AddWithValue("@FromUserId", fromId);
        ///     cmd2.Parameters.AddWithValue("@ToUserId", toId);
        ///     await cmd2.ExecuteNonQueryAsync(ct);
        /// });
        /// // Si cualquier SP falla, ambos hacen rollback.
        /// </code>
        /// </example>
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task> operation,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Igual que ExecuteInTransactionAsync pero retorna un valor.
        /// Util cuando el SP retorna un ID generado u otro resultado.
        ///
        /// <example>
        /// <code>
        /// var newId = await _db.ExecuteInTransactionAsync&lt;Guid&gt;(async (conn, tx, ct) =>
        /// {
        ///     using var cmd = conn.CreateCommand();
        ///     cmd.Transaction = tx;
        ///     cmd.CommandText = "sp_CreateBatchOperation";
        ///     cmd.CommandType = CommandType.StoredProcedure;
        ///     cmd.Parameters.AddWithValue("@Name", batchName);
        ///     var result = await cmd.ExecuteScalarAsync(ct);
        ///     return (Guid)result!;
        /// });
        /// </code>
        /// </example>
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);
    }
}
