using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ResearchHub.Tests;

public sealed class ResearchHubWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DemoPassword = "lab4-test-password";
    private const string JwtKey = "ResearchHub_Testcontainers_JWT_Secret_Key_2026_12345";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("researchhub_test")
        .WithUsername("researchhub_test")
        .WithPassword("researchhub_test_password")
        .Build();

    public string LoginPassword => DemoPassword;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await InitializeDatabaseAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = "ResearchHub.Api",
                ["Jwt:Audience"] = "ResearchHub.Client",
                ["Jwt:Key"] = JwtKey,
                ["Jwt:LifetimeMinutes"] = "60",
                ["Jwt:DemoPassword"] = DemoPassword,
                ["Cors:Origins:0"] = "http://localhost"
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await ExecuteFileAsync("08_seed.sql");
    }

    private async Task InitializeDatabaseAsync()
    {
        foreach (var file in new[]
        {
            "01_schema.sql", "02_constraints.sql", "03_functions_users.sql",
            "04_functions_projects.sql", "05_functions_tasks.sql", "06_views.sql",
            "07_indexes.sql", "08_seed.sql"
        })
        {
            await ExecuteFileAsync(file);
        }
    }

    private async Task ExecuteFileAsync(string file)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Database", file);
        var sql = await File.ReadAllTextAsync(path);
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 60 };
        await command.ExecuteNonQueryAsync();
    }
}
