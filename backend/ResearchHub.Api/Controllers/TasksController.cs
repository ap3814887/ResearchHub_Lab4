using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.Services;

namespace ResearchHub.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskService service) : ControllerBase
{
    [AllowAnonymous, HttpGet("{id:long}")]
    public async Task<ActionResult<TaskResponse>> GetById(long id, CancellationToken ct)
    { var x = await service.GetByIdAsync(id, ct); return x is null ? NotFound() : Ok(x); }

    [Authorize, HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken ct)
    { var x = await service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, x); }

    [Authorize, HttpPut("{id:long}")]
    public async Task<ActionResult<TaskResponse>> Update(long id, UpdateTaskRequest request, CancellationToken ct) => Ok(await service.UpdateAsync(id, request, ct));

    [Authorize, HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    { await service.DeleteAsync(id, ct); return NoContent(); }

    [Authorize, HttpPatch("{id:long}/status")]
    public async Task<ActionResult<TaskStatusChangeResponse>> ChangeStatus(long id, ChangeTaskStatusRequest request, CancellationToken ct) => Ok(await service.ChangeStatusAsync(id, request, ct));

    [Authorize, HttpPatch("{id:long}/assignee")]
    public async Task<IActionResult> Assign(long id, AssignTaskRequest request, CancellationToken ct)
    { await service.AssignAsync(id, request, ct); return NoContent(); }

    [Authorize, HttpPatch("{id:long}/priority")]
    public async Task<IActionResult> ChangePriority(long id, ChangeTaskPriorityRequest request, CancellationToken ct)
    { await service.ChangePriorityAsync(id, request, ct); return NoContent(); }
}
