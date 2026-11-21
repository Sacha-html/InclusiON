using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using InclusiON.Application.Exceptions;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        var (statusCode, errorCode, message) = exception switch
        {
            DbUpdateException dbEx when dbEx.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pgEx
                => MapUniqueViolation(pgEx),

            EntityNotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                ErrorCode.NotFound,
                ex.Message),

            DataAccessException => (
                (int)HttpStatusCode.InternalServerError,
                ErrorCode.InternalError,
                GetMessage(exception, "Error de acceso a datos.")),

            OperationCanceledException => (
                499,
                ErrorCode.Unknown,
                "La solicitud fue cancelada."),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                ErrorCode.InternalError,
                GetMessage(exception, "Ocurrió un error interno en el servidor."))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.ErrorResult(errorCode, message);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (int StatusCode, ErrorCode ErrorCode, string Message) MapUniqueViolation(PostgresException pgEx)
    {
        var constraint = pgEx.ConstraintName ?? string.Empty;

        if (constraint.Contains("DocumentNumber", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.Conflict,
                ErrorCode.DocumentAlreadyExists,
                "El número de documento ya se encuentra registrado en el sistema.");
        }

        if (constraint.Contains("Email", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.Conflict,
                ErrorCode.EmailAlreadyExists,
                "Este email ya se encuentra registrado en el sistema.");
        }

        if (constraint.Contains("LicenseNumber", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.Conflict,
                ErrorCode.LicenseNumberAlreadyExists,
                "Esta matrícula ya se encuentra registrada en el sistema.");
        }

        return (
            (int)HttpStatusCode.Conflict,
            ErrorCode.DuplicateEntry,
            "Ya existe un registro con estos datos únicos.");
    }

    private string GetMessage(Exception exception, string fallback)
    {
        return _environment.IsDevelopment() ? exception.Message : fallback;
    }
}
