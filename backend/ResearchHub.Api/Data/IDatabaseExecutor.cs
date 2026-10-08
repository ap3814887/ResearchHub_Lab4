using Npgsql;
namespace ResearchHub.Api.Data;
public interface IDatabaseExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default);
    Task<T?> ExecuteScalarAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default);
}
