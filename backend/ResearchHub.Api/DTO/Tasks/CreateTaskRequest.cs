using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Tasks;
public sealed class CreateTaskRequest
{
    [Range(1, long.MaxValue)] public long ProjectId { get; init; }
    [Required, StringLength(250)] public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public long? AssigneeId { get; init; }
    [Required] public string Status { get; init; } = "New";
    [Required] public string Priority { get; init; } = "Normal";
    public DateOnly? DueDate { get; init; }
}
