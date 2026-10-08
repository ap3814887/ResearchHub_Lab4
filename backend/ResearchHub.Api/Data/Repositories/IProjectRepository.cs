using ResearchHub.Api.Models;
namespace ResearchHub.Api.Data.Repositories;
public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectRecord>> GetAllAsync(CancellationToken ct);
    Task<ProjectRecord?> GetByIdAsync(long id, CancellationToken ct);
    Task<ProjectRecord> CreateAsync(string name, string? description, long ownerId, CancellationToken ct);
    Task<ProjectRecord> UpdateAsync(long id, string name, string? description, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<ProjectMemberRecord>> GetMembersAsync(long projectId, CancellationToken ct);
    Task<ProjectMemberRecord> AddMemberAsync(long projectId, long userId, string role, CancellationToken ct);
    Task RemoveMemberAsync(long projectId, long userId, CancellationToken ct);
}
