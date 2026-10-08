using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ResearchHub.Api.DTO.Auth;
using ResearchHub.Api.DTO.Users;

namespace ResearchHub.Api.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _o = options.Value;
    public TokenResponse CreateToken(UserResponse user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_o.LifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key));
        var token = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, now, expires, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", expires, user.Id, user.Name);
    }
}
