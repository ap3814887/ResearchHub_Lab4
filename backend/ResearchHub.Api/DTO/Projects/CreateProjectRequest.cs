using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Projects;
public sealed class CreateProjectRequest
{
    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    [Range(1, long.MaxValue)] public long OwnerId { get; init; }
}
