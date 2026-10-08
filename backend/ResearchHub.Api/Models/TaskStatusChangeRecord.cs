namespace ResearchHub.Api.Models;
public sealed record TaskStatusChangeRecord(long Id, string OldStatus, string NewStatus, DateTime UpdatedAt);
