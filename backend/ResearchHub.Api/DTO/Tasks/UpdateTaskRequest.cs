using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Tasks;
public sealed class UpdateTaskRequest
{
    [Required, StringLength(250)] public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public long? AssigneeId { get; init; }
    [Required] public string Priority { get; init; } = "Normal";
    public DateOnly? DueDate { get; init; }
}
