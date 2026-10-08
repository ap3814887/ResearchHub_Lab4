using ResearchHub.Api.Data.Repositories;
using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Projects;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.Mapping;

namespace ResearchHub.Api.Services;
public sealed class ProjectService(IProjectRepository projects, ITaskRepository tasks) : IProjectService
{
    public async Task<PagedResponse<ProjectResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct) => PagedResponse<ProjectResponse>.Create((await projects.GetAllAsync(ct)).Select(x => x.ToResponse()), page, pageSize);
    public async Task<ProjectResponse?> GetByIdAsync(long id, CancellationToken ct) => (await projects.GetByIdAsync(id, ct))?.ToResponse();
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct) => (await projects.CreateAsync(request.Name, request.Description, request.OwnerId, ct)).ToResponse();
    public async Task<ProjectResponse> UpdateAsync(long id, UpdateProjectRequest request, CancellationToken ct) => (await projects.UpdateAsync(id, request.Name, request.Description, ct)).ToResponse();
    public Task DeleteAsync(long id, CancellationToken ct) => projects.DeleteAsync(id, ct);
    public async Task<IReadOnlyList<ProjectMemberResponse>> GetMembersAsync(long id, CancellationToken ct) => (await projects.GetMembersAsync(id, ct)).Select(x => x.ToResponse()).ToList();
    public async Task<ProjectMemberResponse> AddMemberAsync(long id, AddProjectMemberRequest request, CancellationToken ct) => (await projects.AddMemberAsync(id, request.UserId, request.Role, ct)).ToResponse();
    public Task RemoveMemberAsync(long id, long userId, CancellationToken ct) => projects.RemoveMemberAsync(id, userId, ct);
    public async Task<PagedResponse<TaskResponse>> GetTasksAsync(long id, string? status, string? priority, long? assigneeId, int page, int pageSize, CancellationToken ct) => PagedResponse<TaskResponse>.Create((await tasks.GetProjectTasksAsync(id, status, priority, assigneeId, ct)).Select(x => x.ToResponse()), page, pageSize);
    public async Task<IReadOnlyList<TaskStatusCountResponse>> GetTaskStatsAsync(long id, CancellationToken ct) => (await tasks.GetStatusCountsAsync(id, ct)).Select(x => x.ToResponse()).ToList();
}
