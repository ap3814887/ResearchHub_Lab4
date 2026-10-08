using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Api.DTO.Auth;

public sealed class RegisterRequest
{
    [Required, StringLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
