namespace ResearchHub.Api.DTO.Projects;
public sealed record ProjectResponse(long Id, string Name, string? Description, long OwnerId, DateTime CreatedAt);
