using ResearchHub.Api.Data.Repositories;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.Mapping;
namespace ResearchHub.Api.Services;
public sealed class TaskService(ITaskRepository tasks) : ITaskService
{
    public async Task<TaskResponse?> GetByIdAsync(long id, CancellationToken ct) => (await tasks.GetByIdAsync(id, ct))?.ToResponse();
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest r, CancellationToken ct) => (await tasks.CreateAsync(r.ProjectId, r.Title, r.Description, r.AssigneeId, r.Status, r.Priority, r.DueDate, ct)).ToResponse();
    public async Task<TaskResponse> UpdateAsync(long id, UpdateTaskRequest r, CancellationToken ct) => (await tasks.UpdateAsync(id, r.Title, r.Description, r.AssigneeId, r.Priority, r.DueDate, ct)).ToResponse();
    public Task DeleteAsync(long id, CancellationToken ct) => tasks.DeleteAsync(id, ct);
    public async Task<TaskStatusChangeResponse> ChangeStatusAsync(long id, ChangeTaskStatusRequest r, CancellationToken ct) => (await tasks.ChangeStatusAsync(id, r.Status, ct)).ToResponse();
    public Task AssignAsync(long id, AssignTaskRequest r, CancellationToken ct) => tasks.AssignAsync(id, r.AssigneeId, ct);
    public Task ChangePriorityAsync(long id, ChangeTaskPriorityRequest r, CancellationToken ct) => tasks.ChangePriorityAsync(id, r.Priority, ct);
}
