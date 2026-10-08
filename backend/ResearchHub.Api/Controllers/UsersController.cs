using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.DTO.Users;
using ResearchHub.Api.Services;

namespace ResearchHub.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IUserService service) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public Task<PagedResponse<UserResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => service.GetAllAsync(page, pageSize, ct);

    [AllowAnonymous, HttpGet("{id:long}")]
    public async Task<ActionResult<UserResponse>> GetById(long id, CancellationToken ct)
    { var x = await service.GetByIdAsync(id, ct); return x is null ? NotFound() : Ok(x); }

    [Authorize, HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken ct)
    { var x = await service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, x); }

    [Authorize, HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponse>> Update(long id, UpdateUserRequest request, CancellationToken ct) => Ok(await service.UpdateAsync(id, request, ct));

    [Authorize, HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    { await service.DeleteAsync(id, ct); return NoContent(); }

    [AllowAnonymous, HttpGet("{id:long}/tasks")]
    public Task<PagedResponse<UserTaskResponse>> GetTasks(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => service.GetTasksAsync(id, page, pageSize, ct);
}
