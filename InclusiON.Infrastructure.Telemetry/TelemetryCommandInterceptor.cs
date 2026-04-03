using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using InclusiON.Application.Interfaces.Telemetry;

namespace InclusiON.Infrastructure.Telemetry;

public class TelemetryCommandInterceptor : DbCommandInterceptor
{
    private readonly ITelemetryService _telemetry;

    public TelemetryCommandInterceptor(ITelemetryService telemetry)
    {
        _telemetry = telemetry;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command.CommandText, eventData.Duration);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordQueryMetrics(string commandText, TimeSpan duration)
    {
        var operation = GetOperationType(commandText);
        var entity = ExtractEntityName(commandText);
        var durationSeconds = duration.TotalSeconds;

        _telemetry.RecordDatabaseQuery(operation, entity, durationSeconds);
    }

    private static string GetOperationType(string commandText)
    {
        var upperCommand = commandText.Trim().ToUpperInvariant();
        
        if (upperCommand.StartsWith("SELECT")) return "select";
        if (upperCommand.StartsWith("INSERT")) return "insert";
        if (upperCommand.StartsWith("UPDATE")) return "update";
        if (upperCommand.StartsWith("DELETE")) return "delete";
        
        return "other";
    }

    private static string ExtractEntityName(string commandText)
    {
        var upperCommand = commandText.Trim().ToUpperInvariant();
        
        var fromIndex = upperCommand.IndexOf("FROM ");
        if (fromIndex >= 0)
        {
            var afterFrom = commandText.Substring(fromIndex + 5).TrimStart();
            var endIndex = afterFrom.IndexOfAny([' ', '\r', '\n', ',', ')', ';']);
            if (endIndex > 0)
            {
                return afterFrom.Substring(0, endIndex).Replace("[", "").Replace("]", "");
            }
            return afterFrom.Replace("[", "").Replace("]", "");
        }

        var intoIndex = upperCommand.IndexOf("INTO ");
        if (intoIndex >= 0)
        {
            var afterInto = commandText.Substring(intoIndex + 5).TrimStart();
            var endIndex = afterInto.IndexOfAny([' ', '\r', '\n', '(', ';']);
            if (endIndex > 0)
            {
                return afterInto.Substring(0, endIndex).Replace("[", "").Replace("]", "");
            }
            return afterInto.Replace("[", "").Replace("]", "");
        }

        var updateIndex = upperCommand.IndexOf("UPDATE ");
        if (updateIndex >= 0)
        {
            var afterUpdate = commandText.Substring(updateIndex + 7).TrimStart();
            var endIndex = afterUpdate.IndexOfAny([' ', '\r', '\n', ';']);
            if (endIndex > 0)
            {
                return afterUpdate.Substring(0, endIndex).Replace("[", "").Replace("]", "");
            }
            return afterUpdate.Replace("[", "").Replace("]", "");
        }

        return "unknown";
    }
}
