using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Projects;
public sealed class AddProjectMemberRequest
{
    [Range(1, long.MaxValue)] public long UserId { get; init; }
    [Required, StringLength(30)] public string Role { get; init; } = "Member";
}
