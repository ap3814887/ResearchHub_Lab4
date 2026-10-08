using ResearchHub.Api.DTO.Common;
using ResearchHub.Api.DTO.Users;
using ResearchHub.Api.DTO.Tasks;
namespace ResearchHub.Api.Services;
public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<UserResponse?> GetByIdAsync(long id, CancellationToken ct);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<UserResponse> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<PagedResponse<UserTaskResponse>> GetTasksAsync(long id, int page, int pageSize, CancellationToken ct);
}
