using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Projects;
public sealed class UpdateProjectRequest
{
    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
