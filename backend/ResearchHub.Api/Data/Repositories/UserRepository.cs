using Npgsql;
using NpgsqlTypes;
using ResearchHub.Api.Models;
using ResearchHub.Api.Data;

namespace ResearchHub.Api.Data.Repositories;

public sealed class UserRepository(IDatabaseExecutor db) : IUserRepository
{
    public Task<IReadOnlyList<UserRecord>> GetAllAsync(CancellationToken ct) =>
        db.QueryAsync("SELECT * FROM get_users();", Array.Empty<NpgsqlParameter>(), Map, ct);

    public Task<UserRecord?> GetByIdAsync(long id, CancellationToken ct) =>
        db.QuerySingleOrDefaultAsync("SELECT * FROM get_user_by_id(@p_user_id);",
            new[] { P("p_user_id", NpgsqlDbType.Bigint, id) }, Map, ct);

    public async Task<UserRecord> CreateAsync(string name, string email, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM create_user(@p_name, @p_email);",
            new[] { P("p_name", NpgsqlDbType.Varchar, name), P("p_email", NpgsqlDbType.Varchar, email) }, Map, ct))!;

    public async Task<UserRecord> UpdateAsync(long id, string name, string email, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM update_user(@p_user_id, @p_name, @p_email);",
            new[] { P("p_user_id", NpgsqlDbType.Bigint, id), P("p_name", NpgsqlDbType.Varchar, name), P("p_email", NpgsqlDbType.Varchar, email) }, Map, ct))!;

    public async Task DeleteAsync(long id, CancellationToken ct) =>
        _ = await db.ExecuteScalarAsync<bool>("SELECT delete_user(@p_user_id);", new[] { P("p_user_id", NpgsqlDbType.Bigint, id) }, ct);

    private static UserRecord Map(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("id")), r.GetString(r.GetOrdinal("name")), r.GetString(r.GetOrdinal("email")), r.GetDateTime(r.GetOrdinal("created_at")));
    private static NpgsqlParameter P(string n, NpgsqlDbType t, object? v) => new(n, t) { Value = v ?? DBNull.Value };
}
