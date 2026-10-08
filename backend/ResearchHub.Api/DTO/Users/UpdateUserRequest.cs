using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Users;
public sealed class UpdateUserRequest
{
    [Required, StringLength(150)] public string Name { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(255)] public string Email { get; init; } = string.Empty;
}
