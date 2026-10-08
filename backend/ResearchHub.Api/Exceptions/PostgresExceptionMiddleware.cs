using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ResearchHub.Api.Exceptions;

public sealed class PostgresExceptionMiddleware(RequestDelegate next, ILogger<PostgresExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (PostgresException ex)
        {
            var status = MapStatus(ex);
            if (status >= 500) logger.LogError(ex, "PostgreSQL error {SqlState}", ex.SqlState);
            else logger.LogWarning("PostgreSQL business error {SqlState}: {Message}", ex.SqlState, ex.MessageText);
            await WriteProblem(context, status, "PostgreSQL operation rejected", ex.MessageText, ex.SqlState);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled server error");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal server error", "Unexpected server error.", null);
        }
    }

    private static int MapStatus(PostgresException ex) => ex.SqlState switch
    {
        "P0002" => StatusCodes.Status404NotFound,
        "23505" => StatusCodes.Status409Conflict,
        "23514" => StatusCodes.Status400BadRequest,
        "23503" when ex.MessageText.Contains("cannot be deleted", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
        "23503" => StatusCodes.Status400BadRequest,
        "23502" => StatusCodes.Status400BadRequest,
        "22P02" => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static Task WriteProblem(HttpContext context, int status, string title, string detail, string? sqlState)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = context.Request.Path };
        if (sqlState is not null) problem.Extensions["sqlState"] = sqlState;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        return context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }
}
