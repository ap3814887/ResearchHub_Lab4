using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Projects;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.Services;

namespace ResearchHub.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(IProjectService service) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public Task<PagedResponse<ProjectResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => service.GetAllAsync(page, pageSize, ct);

    [AllowAnonymous, HttpGet("{id:long}")]
    public async Task<ActionResult<ProjectResponse>> GetById(long id, CancellationToken ct)
    { var x = await service.GetByIdAsync(id, ct); return x is null ? NotFound() : Ok(x); }

    [Authorize, HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    { var x = await service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, x); }

    [Authorize, HttpPut("{id:long}")]
    public async Task<ActionResult<ProjectResponse>> Update(long id, UpdateProjectRequest request, CancellationToken ct) => Ok(await service.UpdateAsync(id, request, ct));

    [Authorize, HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    { await service.DeleteAsync(id, ct); return NoContent(); }

    [AllowAnonymous, HttpGet("{projectId:long}/members")]
    public Task<IReadOnlyList<ProjectMemberResponse>> GetMembers(long projectId, CancellationToken ct) => service.GetMembersAsync(projectId, ct);

    [Authorize, HttpPost("{projectId:long}/members")]
    public async Task<ActionResult<ProjectMemberResponse>> AddMember(long projectId, AddProjectMemberRequest request, CancellationToken ct)
    { var x = await service.AddMemberAsync(projectId, request, ct); return StatusCode(StatusCodes.Status201Created, x); }

    [Authorize, HttpDelete("{projectId:long}/members/{userId:long}")]
    public async Task<IActionResult> RemoveMember(long projectId, long userId, CancellationToken ct)
    { await service.RemoveMemberAsync(projectId, userId, ct); return NoContent(); }

    [AllowAnonymous, HttpGet("{projectId:long}/tasks")]
    public Task<PagedResponse<TaskResponse>> GetTasks(long projectId, [FromQuery] string? status, [FromQuery] string? priority, [FromQuery] long? assigneeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => service.GetTasksAsync(projectId, status, priority, assigneeId, page, pageSize, ct);

    [AllowAnonymous, HttpGet("{projectId:long}/task-stats")]
    public Task<IReadOnlyList<TaskStatusCountResponse>> GetTaskStats(long projectId, CancellationToken ct) => service.GetTaskStatsAsync(projectId, ct);
}
