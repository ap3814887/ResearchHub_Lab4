using System.ComponentModel.DataAnnotations;
namespace ResearchHub.Api.DTO.Auth;
public sealed class LoginRequest
{
    [Range(1, long.MaxValue)] public long UserId { get; init; }
    [Required] public string Password { get; init; } = string.Empty;
}
