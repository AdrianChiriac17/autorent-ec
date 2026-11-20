using AutoRent.Application.Abstractions;
using AutoRent.Infrastructure.Authentication;
using AutoRent.Infrastructure.Persistence;
using AutoRent.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, bool seedDevelopmentData)
    {
        var connectionString = configuration.GetConnectionString("AutoRent")
            ?? throw new InvalidOperationException(
                "Connection string 'AutoRent' is missing. Set ConnectionStrings__AutoRent (see .env.example).");

        var seed = seedDevelopmentData ? SeedOptions.FromConfiguration(configuration) : null;

        services.AddDbContextFactory<AutoRentDbContext>(options =>
            AutoRentDbOptions.Configure(options, connectionString, seed));
        services.AddScoped<IAutoRentDbContextProvider, AutoRentDbContextProvider>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        return services;
    }
}
