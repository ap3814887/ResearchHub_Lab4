using Npgsql;

namespace ResearchHub.Api.Data;

public sealed class DatabaseExecutor(IDbConnectionFactory connectionFactory, ILogger<DatabaseExecutor> logger) : IDatabaseExecutor
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(sql, connection, parameters);
        logger.LogDebug("Executing PostgreSQL query: {Sql}", sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<T>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(map(reader));
        return result;
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        var rows = await QueryAsync(sql, parameters, map, cancellationToken);
        return rows.Count == 0 ? default : rows[0];
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(sql, connection, parameters);
        logger.LogDebug("Executing PostgreSQL scalar command: {Sql}", sql);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null || value is DBNull) return default;
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(sql, connection, parameters);
        logger.LogDebug("Executing PostgreSQL non-query command: {Sql}", sql);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static NpgsqlCommand CreateCommand(string sql, NpgsqlConnection connection, IEnumerable<NpgsqlParameter> parameters)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var parameter in parameters) command.Parameters.Add(parameter);
        return command;
    }
}
