namespace ResearchHub.Api.DTO.Tasks;
public sealed record TaskStatusChangeResponse(long Id, string OldStatus, string NewStatus, DateTime UpdatedAt);
