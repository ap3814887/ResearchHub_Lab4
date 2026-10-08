using Npgsql;

namespace ResearchHub.Api.Data;

public sealed class DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger) : IDbConnectionFactory
{
    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        logger.LogDebug("PostgreSQL connection opened to {Database}", connection.Database);
        return connection;
    }
}
