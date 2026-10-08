using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Projects;
using ResearchHub.Api.DTO.Tasks;
namespace ResearchHub.Api.Services;
public interface IProjectService
{
    Task<PagedResponse<ProjectResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<ProjectResponse?> GetByIdAsync(long id, CancellationToken ct);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct);
    Task<ProjectResponse> UpdateAsync(long id, UpdateProjectRequest request, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<ProjectMemberResponse>> GetMembersAsync(long id, CancellationToken ct);
    Task<ProjectMemberResponse> AddMemberAsync(long id, AddProjectMemberRequest request, CancellationToken ct);
    Task RemoveMemberAsync(long id, long userId, CancellationToken ct);
    Task<PagedResponse<TaskResponse>> GetTasksAsync(long id, string? status, string? priority, long? assigneeId, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<TaskStatusCountResponse>> GetTaskStatsAsync(long id, CancellationToken ct);
}
