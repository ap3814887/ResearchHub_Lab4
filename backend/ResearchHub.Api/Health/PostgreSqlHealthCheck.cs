using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using ResearchHub.Api.Data;
namespace ResearchHub.Api.Health;
public sealed class PostgreSqlHealthCheck(IDbConnectionFactory factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await factory.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1;", connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) == 1 ? HealthCheckResult.Healthy("PostgreSQL is reachable") : HealthCheckResult.Unhealthy("Unexpected PostgreSQL health result");
        }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("PostgreSQL is unavailable", ex); }
    }
}
