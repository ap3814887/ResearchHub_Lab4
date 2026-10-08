using ResearchHub.Api.Models;
namespace ResearchHub.Api.Data.Repositories;
public interface IUserRepository
{
    Task<IReadOnlyList<UserRecord>> GetAllAsync(CancellationToken ct);
    Task<UserRecord?> GetByIdAsync(long id, CancellationToken ct);
    Task<UserRecord> CreateAsync(string name, string email, CancellationToken ct);
    Task<UserRecord> UpdateAsync(long id, string name, string email, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
}
