namespace ResearchHub.Api.DTO.Projects;
public sealed record ProjectMemberResponse(long ProjectId, long UserId, string? UserName, string? Email, string Role, DateTime JoinedAt);
