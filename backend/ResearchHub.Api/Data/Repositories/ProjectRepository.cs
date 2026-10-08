using Npgsql;
using NpgsqlTypes;
using ResearchHub.Api.Models;
using ResearchHub.Api.Data;

namespace ResearchHub.Api.Data.Repositories;

public sealed class ProjectRepository(IDatabaseExecutor db) : IProjectRepository
{
    public Task<IReadOnlyList<ProjectRecord>> GetAllAsync(CancellationToken ct) => db.QueryAsync("SELECT * FROM get_projects();", Array.Empty<NpgsqlParameter>(), MapProject, ct);
    public Task<ProjectRecord?> GetByIdAsync(long id, CancellationToken ct) => db.QuerySingleOrDefaultAsync("SELECT * FROM get_project_by_id(@p_project_id);", new[] { P("p_project_id", NpgsqlDbType.Bigint, id) }, MapProject, ct);
    public async Task<ProjectRecord> CreateAsync(string name, string? description, long ownerId, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM create_project(@p_name, @p_description, @p_owner_id);",
            new[] { P("p_name", NpgsqlDbType.Varchar, name), P("p_description", NpgsqlDbType.Text, description), P("p_owner_id", NpgsqlDbType.Bigint, ownerId) }, MapProject, ct))!;
    public async Task<ProjectRecord> UpdateAsync(long id, string name, string? description, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM update_project(@p_project_id, @p_name, @p_description);",
            new[] { P("p_project_id", NpgsqlDbType.Bigint, id), P("p_name", NpgsqlDbType.Varchar, name), P("p_description", NpgsqlDbType.Text, description) }, MapProject, ct))!;
    public async Task DeleteAsync(long id, CancellationToken ct) => _ = await db.ExecuteScalarAsync<bool>("SELECT delete_project(@p_project_id);", new[] { P("p_project_id", NpgsqlDbType.Bigint, id) }, ct);
    public Task<IReadOnlyList<ProjectMemberRecord>> GetMembersAsync(long projectId, CancellationToken ct) => db.QueryAsync("SELECT * FROM get_project_members(@p_project_id);", new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId) }, MapMemberFull, ct);
    public async Task<ProjectMemberRecord> AddMemberAsync(long projectId, long userId, string role, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM add_project_member(@p_project_id, @p_user_id, @p_role);",
            new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId), P("p_user_id", NpgsqlDbType.Bigint, userId), P("p_role", NpgsqlDbType.Varchar, role) }, MapMemberBasic, ct))!;
    public async Task RemoveMemberAsync(long projectId, long userId, CancellationToken ct) => _ = await db.ExecuteScalarAsync<bool>("SELECT remove_project_member(@p_project_id, @p_user_id);", new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId), P("p_user_id", NpgsqlDbType.Bigint, userId) }, ct);

    private static ProjectRecord MapProject(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("id")), r.GetString(r.GetOrdinal("name")), r.GetNullableString("description"), r.GetInt64(r.GetOrdinal("owner_id")), r.GetDateTime(r.GetOrdinal("created_at")));
    private static ProjectMemberRecord MapMemberFull(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("project_id")), r.GetInt64(r.GetOrdinal("user_id")), r.GetNullableString("user_name"), r.GetNullableString("email"), r.GetString(r.GetOrdinal("role")), r.GetDateTime(r.GetOrdinal("joined_at")));
    private static ProjectMemberRecord MapMemberBasic(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("project_id")), r.GetInt64(r.GetOrdinal("user_id")), null, null, r.GetString(r.GetOrdinal("role")), r.GetDateTime(r.GetOrdinal("joined_at")));
    private static NpgsqlParameter P(string n, NpgsqlDbType t, object? v) => new(n, t) { Value = v ?? DBNull.Value };
}
