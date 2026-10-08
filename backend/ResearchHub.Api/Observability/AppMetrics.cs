using System.Diagnostics;
using System.Threading;

namespace ResearchHub.Api.Observability;

public sealed class AppMetrics
{
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private long _requests;
    private long _errors;
    private long _durationMs;

    public void Record(int statusCode, long elapsedMs)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Add(ref _durationMs, elapsedMs);
        if (statusCode >= 400) Interlocked.Increment(ref _errors);
    }

    public string RenderPrometheus()
    {
        var requests = Interlocked.Read(ref _requests);
        var errors = Interlocked.Read(ref _errors);
        var duration = Interlocked.Read(ref _durationMs);
        return string.Join('\n', new[]
        {
            "# HELP researchhub_up Application availability flag.",
            "# TYPE researchhub_up gauge",
            "researchhub_up 1",
            "# HELP researchhub_uptime_seconds Process uptime in seconds.",
            "# TYPE researchhub_uptime_seconds gauge",
            $"researchhub_uptime_seconds {_uptime.Elapsed.TotalSeconds:F0}",
            "# HELP researchhub_http_requests_total Total HTTP requests processed.",
            "# TYPE researchhub_http_requests_total counter",
            $"researchhub_http_requests_total {requests}",
            "# HELP researchhub_http_errors_total Total HTTP responses with status >= 400.",
            "# TYPE researchhub_http_errors_total counter",
            $"researchhub_http_errors_total {errors}",
            "# HELP researchhub_http_request_duration_ms_total Accumulated request duration in milliseconds.",
            "# TYPE researchhub_http_request_duration_ms_total counter",
            $"researchhub_http_request_duration_ms_total {duration}",
            string.Empty
        });
    }
}
