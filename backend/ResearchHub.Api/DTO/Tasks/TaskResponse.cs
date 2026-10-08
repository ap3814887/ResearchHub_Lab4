namespace ResearchHub.Api.DTO.Tasks;
public sealed record TaskResponse(long Id, long ProjectId, string Title, string? Description, long? AssigneeId, string Status, string Priority, DateOnly? DueDate, DateTime CreatedAt, DateTime UpdatedAt);
