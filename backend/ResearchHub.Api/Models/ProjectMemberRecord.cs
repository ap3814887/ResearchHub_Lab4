namespace ResearchHub.Api.Models;
public sealed record ProjectMemberRecord(long ProjectId, long UserId, string? UserName, string? Email, string Role, DateTime JoinedAt);
