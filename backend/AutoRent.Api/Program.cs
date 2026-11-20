using AutoRent.Api.Extensions;
using AutoRent.Api.Middleware;
using AutoRent.Application;
using AutoRent.Infrastructure;
using AutoRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Local development: pick up the repository's .env (real environment variables take precedence).
DotEnvFile.LoadNearest();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddApplication()
    .AddInfrastructure(config, seedDevelopmentData: config.GetValue<bool>("Database:SeedDevelopmentData"))
    .AddApiProblemDetails()
    .AddExceptionHandler<AppExceptionHandler>()
    .AddJwtAuthentication(config)
    .AddAuthRateLimiting(config)
    .AddFrontendCors(config)
    .AddApiControllers()
    .AddOpenApi();

var app = builder.Build();

if (config.GetValue<bool>("Database:MigrateOnStartup"))
{
    // Applies pending migrations (never drops data) and runs the idempotent dev seed when enabled.
    await using var db = await app.Services.GetRequiredService<IDbContextFactory<AutoRentDbContext>>().CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(ApiServiceExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in the integration tests.</summary>
public partial class Program;
