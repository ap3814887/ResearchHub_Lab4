namespace ResearchHub.Api.Models;
public sealed record UserTaskRecord(long Id, long ProjectId, string ProjectName, string Title, string Status, string Priority, DateOnly? DueDate);
