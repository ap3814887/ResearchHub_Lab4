namespace ResearchHub.Api.DTO.Tasks;
public sealed record UserTaskResponse(long Id, long ProjectId, string ProjectName, string Title, string Status, string Priority, DateOnly? DueDate);
