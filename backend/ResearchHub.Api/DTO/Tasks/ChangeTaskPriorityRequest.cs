using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Tasks;
public sealed class ChangeTaskPriorityRequest
{
    [Required] public string Priority { get; init; } = string.Empty;
}
