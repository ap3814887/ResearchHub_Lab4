using ResearchHub.Api.DTO.Auth;
using ResearchHub.Api.DTO.Users;
namespace ResearchHub.Api.Auth;
public interface IJwtTokenService { TokenResponse CreateToken(UserResponse user); }
