using Npgsql;
namespace ResearchHub.Api.Data;
public interface IDbConnectionFactory
{
    Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
