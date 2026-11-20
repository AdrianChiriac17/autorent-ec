using AutoRent.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserAccessService>();
        services.AddScoped<UserAdminService>();
        return services;
    }
}
