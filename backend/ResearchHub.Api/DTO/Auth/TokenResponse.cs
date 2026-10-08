namespace ResearchHub.Api.DTO.Auth;
public sealed record TokenResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc, long UserId, string Name);
