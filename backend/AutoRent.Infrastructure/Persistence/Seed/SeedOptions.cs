using Microsoft.Extensions.Configuration;

namespace AutoRent.Infrastructure.Persistence.Seed;

/// <summary>
/// Passwords for the development seed accounts. They come from configuration
/// (<c>Seed__AdminPassword</c> / <c>Seed__UserPassword</c> in .env) and are never stored in source control.
/// </summary>
public sealed record SeedOptions(string AdminPassword, string UserPassword)
{
    public const string AdminEmail = "admin@autorent.local";

    public static SeedOptions FromConfiguration(IConfiguration configuration) => new(
        Require(configuration, "Seed:AdminPassword"),
        Require(configuration, "Seed:UserPassword"));

    private static string Require(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(
                $"'{key}' is required when Database:SeedDevelopmentData is on. Set {key.Replace(":", "__", StringComparison.Ordinal)} in .env (see .env.example).")
            : value;
    }
}
