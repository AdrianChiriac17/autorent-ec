using AutoRent.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Infrastructure.Persistence;

/// <summary>Single place that configures the Npgsql context.</summary>
internal static class AutoRentDbOptions
{
    /// <param name="seed">When set, the idempotent development seed runs after migrations are applied.</param>
    public static void Configure(DbContextOptionsBuilder options, string connectionString, SeedOptions? seed)
    {
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(AutoRentDbContext).Assembly.FullName));

        if (seed is not null)
        {
            // Runs after Database.Migrate()/MigrateAsync() and `dotnet ef database update`.
            options.UseSeeding((context, _) => DevelopmentSeeder.SeedAsync(context, seed, CancellationToken.None).GetAwaiter().GetResult());
            options.UseAsyncSeeding((context, _, ct) => DevelopmentSeeder.SeedAsync(context, seed, ct));
        }
    }
}
