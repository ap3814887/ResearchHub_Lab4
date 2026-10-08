using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResearchHub.Api.Auth;
using ResearchHub.Api.Data;
using ResearchHub.Api.Data.Repositories;
using ResearchHub.Api.Exceptions;
using ResearchHub.Api.Health;
using ResearchHub.Api.Middleware;
using ResearchHub.Api.Observability;
using ResearchHub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ResearchHub API",
        Version = "v1",
        Description = "ResearchHub — ЛР4: тестирование, Docker Compose, health checks и метрики"
    });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Получите JWT через POST /api/auth/token и вставьте токен."
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(x => !string.IsNullOrWhiteSpace(x.Key) && x.Key.Length >= 32, "Jwt:Key must be at least 32 characters")
    .Validate(x => !string.IsNullOrWhiteSpace(x.DemoPassword), "Jwt:DemoPassword must be configured")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var jwt = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<IDatabaseExecutor, DatabaseExecutor>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<AppMetrics>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API process is running"), tags: new[] { "live" })
    .AddCheck<PostgreSqlHealthCheck>("postgresql", tags: new[] { "ready", "db", "postgresql" });

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
{
    if (origins.Length > 0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

app.UseMiddleware<MetricsMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<PostgresExceptionMiddleware>();

// В ЛР4 Swagger доступен и в Docker-режиме для демонстрации API на защите.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

static Task WriteHealth(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var payload = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    });
    return context.Response.WriteAsync(payload);
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = x => x.Tags.Contains("live"),
    ResponseWriter = WriteHealth
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = x => x.Tags.Contains("ready"),
    ResponseWriter = WriteHealth
});
app.MapHealthChecks("/health/postgres", new HealthCheckOptions
{
    Predicate = x => x.Name == "postgresql",
    ResponseWriter = WriteHealth
});

app.MapGet("/metrics", (AppMetrics metrics) => Results.Text(metrics.RenderPrometheus(), "text/plain; version=0.0.4"))
    .AllowAnonymous();

app.Run();

// Нужен WebApplicationFactory<Program> в интеграционных xUnit-тестах.
public partial class Program { }
