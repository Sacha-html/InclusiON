using Npgsql;

const string cs = "Host=192.168.0.17;Port=5433;Database=inclusion_dev;Username=postgres;Password=postgres";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();

await using var delCmd = conn.CreateCommand();
delCmd.CommandText = "TRUNCATE TABLE \"ActivitySessions\" RESTART IDENTITY;";
await delCmd.ExecuteNonQueryAsync();
Console.WriteLine("Tabla ActivitySessions vaciada para re-seeding secuencial.");
