namespace ResearchHub.Api.Models;
public sealed record ProjectRecord(long Id, string Name, string? Description, long OwnerId, DateTime CreatedAt);
