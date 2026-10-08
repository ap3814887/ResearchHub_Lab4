using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Tasks;
public sealed class ChangeTaskStatusRequest
{
    [Required] public string Status { get; init; } = string.Empty;
}
