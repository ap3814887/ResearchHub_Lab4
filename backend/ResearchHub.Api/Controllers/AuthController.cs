using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResearchHub.Api.Auth;
using ResearchHub.Api.DTO.Auth;
using ResearchHub.Api.DTO.Users;
using ResearchHub.Api.Services;

namespace ResearchHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserService users, IJwtTokenService tokens, IOptions<JwtOptions> jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Token(LoginRequest request, CancellationToken ct)
    {
        if (!string.Equals(request.Password, jwt.Value.DemoPassword, StringComparison.Ordinal))
            return Unauthorized(new { message = "Invalid credentials" });

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(tokens.CreateToken(user));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        // В учебной схеме ЛР2 отдельное хранение паролей не добавляется.
        // Регистрация использует тот же демонстрационный пароль, что и JWT-вход.
        if (!string.Equals(request.Password, jwt.Value.DemoPassword, StringComparison.Ordinal))
            return BadRequest(new { message = "Invalid registration password" });

        var created = await users.CreateAsync(new CreateUserRequest
        {
            Name = request.Name,
            Email = request.Email
        }, ct);

        return StatusCode(StatusCodes.Status201Created, tokens.CreateToken(created));
    }
}
