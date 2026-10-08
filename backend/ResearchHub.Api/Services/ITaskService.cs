using ResearchHub.Api.DTO.Tasks;
namespace ResearchHub.Api.Services;
public interface ITaskService
{
    Task<TaskResponse?> GetByIdAsync(long id, CancellationToken ct);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct);
    Task<TaskResponse> UpdateAsync(long id, UpdateTaskRequest request, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<TaskStatusChangeResponse> ChangeStatusAsync(long id, ChangeTaskStatusRequest request, CancellationToken ct);
    Task AssignAsync(long id, AssignTaskRequest request, CancellationToken ct);
    Task ChangePriorityAsync(long id, ChangeTaskPriorityRequest request, CancellationToken ct);
}
