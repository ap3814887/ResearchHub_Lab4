using ResearchHub.Api.Models;
namespace ResearchHub.Api.Data.Repositories;
public interface ITaskRepository
{
    Task<TaskRecord?> GetByIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<TaskRecord>> GetProjectTasksAsync(long projectId, string? status, string? priority, long? assigneeId, CancellationToken ct);
    Task<TaskRecord> CreateAsync(long projectId, string title, string? description, long? assigneeId, string status, string priority, DateOnly? dueDate, CancellationToken ct);
    Task<TaskRecord> UpdateAsync(long id, string title, string? description, long? assigneeId, string priority, DateOnly? dueDate, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<TaskStatusChangeRecord> ChangeStatusAsync(long id, string status, CancellationToken ct);
    Task AssignAsync(long id, long? assigneeId, CancellationToken ct);
    Task ChangePriorityAsync(long id, string priority, CancellationToken ct);
    Task<IReadOnlyList<UserTaskRecord>> GetUserTasksAsync(long userId, CancellationToken ct);
    Task<IReadOnlyList<TaskStatusCountRecord>> GetStatusCountsAsync(long? projectId, CancellationToken ct);
}
