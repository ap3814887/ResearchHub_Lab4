using Npgsql;
using NpgsqlTypes;
using ResearchHub.Api.Models;
using ResearchHub.Api.Data;

namespace ResearchHub.Api.Data.Repositories;

public sealed class TaskRepository(IDatabaseExecutor db) : ITaskRepository
{
    public Task<TaskRecord?> GetByIdAsync(long id, CancellationToken ct) => db.QuerySingleOrDefaultAsync("SELECT * FROM get_task_by_id(@p_task_id);", new[] { P("p_task_id", NpgsqlDbType.Bigint, id) }, MapTask, ct);
    public Task<IReadOnlyList<TaskRecord>> GetProjectTasksAsync(long projectId, string? status, string? priority, long? assigneeId, CancellationToken ct) =>
        db.QueryAsync("SELECT * FROM get_project_tasks(@p_project_id, @p_status, @p_priority, @p_assignee_id);",
            new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId), P("p_status", NpgsqlDbType.Varchar, status), P("p_priority", NpgsqlDbType.Varchar, priority), P("p_assignee_id", NpgsqlDbType.Bigint, assigneeId) }, MapTask, ct);
    public async Task<TaskRecord> CreateAsync(long projectId, string title, string? description, long? assigneeId, string status, string priority, DateOnly? dueDate, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM create_task(@p_project_id, @p_title, @p_description, @p_assignee_id, @p_status, @p_priority, @p_due_date);",
            new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId), P("p_title", NpgsqlDbType.Varchar, title), P("p_description", NpgsqlDbType.Text, description), P("p_assignee_id", NpgsqlDbType.Bigint, assigneeId), P("p_status", NpgsqlDbType.Varchar, status), P("p_priority", NpgsqlDbType.Varchar, priority), P("p_due_date", NpgsqlDbType.Date, dueDate) }, MapTask, ct))!;
    public async Task<TaskRecord> UpdateAsync(long id, string title, string? description, long? assigneeId, string priority, DateOnly? dueDate, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM update_task(@p_task_id, @p_title, @p_description, @p_assignee_id, @p_priority, @p_due_date);",
            new[] { P("p_task_id", NpgsqlDbType.Bigint, id), P("p_title", NpgsqlDbType.Varchar, title), P("p_description", NpgsqlDbType.Text, description), P("p_assignee_id", NpgsqlDbType.Bigint, assigneeId), P("p_priority", NpgsqlDbType.Varchar, priority), P("p_due_date", NpgsqlDbType.Date, dueDate) }, MapTask, ct))!;
    public async Task DeleteAsync(long id, CancellationToken ct) => _ = await db.ExecuteScalarAsync<bool>("SELECT delete_task(@p_task_id);", new[] { P("p_task_id", NpgsqlDbType.Bigint, id) }, ct);
    public async Task<TaskStatusChangeRecord> ChangeStatusAsync(long id, string status, CancellationToken ct) =>
        (await db.QuerySingleOrDefaultAsync("SELECT * FROM change_task_status(@p_task_id, @p_new_status);", new[] { P("p_task_id", NpgsqlDbType.Bigint, id), P("p_new_status", NpgsqlDbType.Varchar, status) }, MapStatusChange, ct))!;
    public async Task AssignAsync(long id, long? assigneeId, CancellationToken ct) => _ = await db.QuerySingleOrDefaultAsync("SELECT * FROM assign_task(@p_task_id, @p_assignee_id);", new[] { P("p_task_id", NpgsqlDbType.Bigint, id), P("p_assignee_id", NpgsqlDbType.Bigint, assigneeId) }, r => r.GetInt64(r.GetOrdinal("id")), ct);
    public Task ChangePriorityAsync(long id, string priority, CancellationToken ct) => db.ExecuteNonQueryAsync("CALL change_task_priority(@p_task_id, @p_new_priority);", new[] { P("p_task_id", NpgsqlDbType.Bigint, id), P("p_new_priority", NpgsqlDbType.Varchar, priority) }, ct);
    public Task<IReadOnlyList<UserTaskRecord>> GetUserTasksAsync(long userId, CancellationToken ct) => db.QueryAsync("SELECT * FROM get_user_tasks(@p_user_id);", new[] { P("p_user_id", NpgsqlDbType.Bigint, userId) }, MapUserTask, ct);
    public Task<IReadOnlyList<TaskStatusCountRecord>> GetStatusCountsAsync(long? projectId, CancellationToken ct) => db.QueryAsync("SELECT * FROM get_task_count_by_status(@p_project_id);", new[] { P("p_project_id", NpgsqlDbType.Bigint, projectId) }, MapCount, ct);

    private static TaskRecord MapTask(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("id")), r.GetInt64(r.GetOrdinal("project_id")), r.GetString(r.GetOrdinal("title")), r.GetNullableString("description"), r.GetNullableInt64("assignee_id"), r.GetString(r.GetOrdinal("status")), r.GetString(r.GetOrdinal("priority")), r.GetNullableDateOnly("due_date"), r.GetDateTime(r.GetOrdinal("created_at")), r.GetDateTime(r.GetOrdinal("updated_at")));
    private static TaskStatusChangeRecord MapStatusChange(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("id")), r.GetString(r.GetOrdinal("old_status")), r.GetString(r.GetOrdinal("new_status")), r.GetDateTime(r.GetOrdinal("updated_at")));
    private static UserTaskRecord MapUserTask(NpgsqlDataReader r) => new(r.GetInt64(r.GetOrdinal("id")), r.GetInt64(r.GetOrdinal("project_id")), r.GetString(r.GetOrdinal("project_name")), r.GetString(r.GetOrdinal("title")), r.GetString(r.GetOrdinal("status")), r.GetString(r.GetOrdinal("priority")), r.GetNullableDateOnly("due_date"));
    private static TaskStatusCountRecord MapCount(NpgsqlDataReader r) => new(r.GetString(r.GetOrdinal("status")), r.GetInt64(r.GetOrdinal("task_count")));
    private static NpgsqlParameter P(string n, NpgsqlDbType t, object? v) => new(n, t) { Value = v ?? DBNull.Value };
}
