namespace ResearchHub.Api.Auth;
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int LifetimeMinutes { get; init; } = 60;
    public string DemoPassword { get; init; } = string.Empty;
}
