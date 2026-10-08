namespace ResearchHub.Api.Models;
public sealed record TaskRecord(
    long Id,
    long ProjectId,
    string Title,
    string? Description,
    long? AssigneeId,
    string Status,
    string Priority,
    DateOnly? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);
