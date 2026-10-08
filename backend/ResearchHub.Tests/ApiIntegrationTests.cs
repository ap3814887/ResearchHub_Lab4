using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResearchHub.Tests;

public sealed class ApiIntegrationTests : IClassFixture<ResearchHubWebApplicationFactory>
{
    private readonly ResearchHubWebApplicationFactory _factory;

    public ApiIntegrationTests(ResearchHubWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetProjects_Returns200()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/projects?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MissingProject_Returns404()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/projects/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_Returns201()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await AuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "Integration project",
            description = "Created by xUnit + Testcontainers",
            ownerId = 1
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateMember_PostgreSqlRule_Returns409()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await AuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("/api/projects/1/members", new { userId = 2, role = "Member" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("already a member", body.ToLowerInvariant());
        Assert.Contains("23505", body);
    }

    [Fact]
    public async Task AssignNonMember_PostgreSqlRule_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await AuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            projectId = 1,
            title = "Invalid assignment",
            description = "User 5 is not a member of project 1",
            assigneeId = 5,
            status = "New",
            priority = "Normal",
            dueDate = (string?)null
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Assignee must be a member", body);
        Assert.Contains("23514", body);
    }

    [Fact]
    public async Task ChangeStatus_Returns200()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await AuthorizedClientAsync();
        var response = await client.PatchAsJsonAsync("/api/tasks/2/status", new { status = "Done" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Done", json.RootElement.GetProperty("newStatus").GetString());
    }

    [Fact]
    public async Task ForbiddenCancelledToDone_IsMappedFromPostgreSql()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await AuthorizedClientAsync();
        var response = await client.PatchAsJsonAsync("/api/tasks/15/status", new { status = "Done" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cancelled task cannot be moved to Done", body);
    }

    [Fact]
    public async Task HealthReady_Returns200()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }

    [Fact]
    public async Task Metrics_ReturnPrometheusText()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        _ = await client.GetAsync("/api/projects");
        var response = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("researchhub_http_requests_total", body);
        Assert.Contains("researchhub_up 1", body);
    }

    private async Task<HttpClient> AuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/token", new
        {
            userId = 1,
            password = _factory.LoginPassword
        });
        response.EnsureSuccessStatusCode();
        using var tokenJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = tokenJson.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
