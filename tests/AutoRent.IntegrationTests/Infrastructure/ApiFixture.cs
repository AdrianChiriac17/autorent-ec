using AutoRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AutoRent.IntegrationTests.Infrastructure;

/// <summary>
/// One real PostgreSQL 17 container and one API instance shared by every integration test.
/// The API applies the migration and the development seed on startup, exactly like local dev.
/// Requires Docker.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    // Generated per test run: no credentials or keys live in the repository.
    public string AdminPassword { get; } = NewSecret();
    public string UserPassword { get; } = NewSecret();
    private readonly string _signingKey = NewSecret() + NewSecret();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("autorent_tests")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task<AutoRentDbContext> CreateDbContextAsync() =>
        await _factory.Services.GetRequiredService<IDbContextFactory<AutoRentDbContext>>().CreateDbContextAsync();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program reads configuration before the host is built, so settings are passed as
        // environment variables. They also take precedence over any developer .env file.
        Environment.SetEnvironmentVariable("ConnectionStrings__AutoRent", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Issuer", "autorent-api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "autorent-web");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", _signingKey);
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminPassword);
        Environment.SetEnvironmentVariable("Seed__UserPassword", UserPassword);
        Environment.SetEnvironmentVariable("Database__MigrateOnStartup", "true");
        Environment.SetEnvironmentVariable("Database__SeedDevelopmentData", "true");
        Environment.SetEnvironmentVariable("RateLimiting__AuthPermitPerMinute", "10000");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseEnvironment("Testing"));
        _ = _factory.Server; // start the host now, so the migration runs once here
    }

    private static string NewSecret() => Guid.NewGuid().ToString("N");

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}
