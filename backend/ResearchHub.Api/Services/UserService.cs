using ResearchHub.Api.Data.Repositories;
using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Users;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.Mapping;

namespace ResearchHub.Api.Services;
public sealed class UserService(IUserRepository users, ITaskRepository tasks) : IUserService
{
    public async Task<PagedResponse<UserResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct) => PagedResponse<UserResponse>.Create((await users.GetAllAsync(ct)).Select(x => x.ToResponse()), page, pageSize);
    public async Task<UserResponse?> GetByIdAsync(long id, CancellationToken ct) => (await users.GetByIdAsync(id, ct))?.ToResponse();
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct) => (await users.CreateAsync(request.Name, request.Email, ct)).ToResponse();
    public async Task<UserResponse> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct) => (await users.UpdateAsync(id, request.Name, request.Email, ct)).ToResponse();
    public Task DeleteAsync(long id, CancellationToken ct) => users.DeleteAsync(id, ct);
    public async Task<PagedResponse<UserTaskResponse>> GetTasksAsync(long id, int page, int pageSize, CancellationToken ct) => PagedResponse<UserTaskResponse>.Create((await tasks.GetUserTasksAsync(id, ct)).Select(x => x.ToResponse()), page, pageSize);
}
