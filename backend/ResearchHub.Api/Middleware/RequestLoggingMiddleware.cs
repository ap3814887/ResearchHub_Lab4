using System.Diagnostics;
namespace ResearchHub.Api.Middleware;
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try { await next(context); }
        finally
        {
            sw.Stop();
            logger.LogInformation("HTTP {Method} {Path} -> {StatusCode} in {Elapsed} ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
