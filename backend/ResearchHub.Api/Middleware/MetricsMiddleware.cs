using System.Diagnostics;
using ResearchHub.Api.Observability;

namespace ResearchHub.Api.Middleware;

public sealed class MetricsMiddleware(RequestDelegate next, AppMetrics metrics)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            metrics.Record(context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
