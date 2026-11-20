using System.Text.Json;
using System.Threading.RateLimiting;
using AutoRent.Application.Abstractions;
using AutoRent.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AutoRent.Api.Extensions;

public static class ApiServiceExtensions
{
    public const string AuthRateLimitPolicy = "auth";
    public const string FrontendCorsPolicy = "frontend";

    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
            ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ctx =>
            {
                // DataAnnotations failures get the same stable code shape as every other error.
                // Keys are camelCased to match the JSON field names, so the frontend can map them to inputs.
                var errors = ctx.ModelState
                    .Where(e => e.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        e => ToCamelCasePath(e.Key),
                        e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

                var problem = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more fields are invalid.",
                };
                problem.Extensions["code"] = "validation.failed";
                problem.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
            });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtAccessTokenIssuer.CreateSigningKey(jwt.SigningKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Name,
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        await WriteProblemAsync(ctx.HttpContext, StatusCodes.Status401Unauthorized,
                            "auth.unauthenticated", "Authentication is required.");
                    },
                    OnForbidden = ctx => WriteProblemAsync(ctx.HttpContext, StatusCodes.Status403Forbidden,
                        "auth.forbidden", "You do not have access to this resource."),
                };
            });

        services.AddScoped<IAuthorizationHandler, AccountAccessHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(AccessPolicies.ActiveUser, p => p.RequireAuthenticatedUser().AddRequirements(new AccountAccessRequirement(false)))
            .AddPolicy(AccessPolicies.Admin, p => p.RequireAuthenticatedUser().AddRequirements(new AccountAccessRequirement(true)))
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AccountAccessRequirement(false))
                .Build());

        return services;
    }

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitPerMinute = configuration.GetValue("RateLimiting:AuthPermitPerMinute", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (ctx, _) => new ValueTask(WriteProblemAsync(ctx.HttpContext,
                StatusCodes.Status429TooManyRequests, "auth.rate_limited", "Too many attempts. Try again in a minute."));

            // Per client IP: slows down password guessing on login and mass account creation on register.
            options.AddPolicy(AuthRateLimitPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });

        return services;
    }

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration["Cors:AllowedOrigins"] ?? string.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    private static string ToCamelCasePath(string key) =>
        string.Join('.', key.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));

    private static Task WriteProblemAsync(HttpContext http, int status, string code, string title)
    {
        var problem = new ProblemDetails { Status = status, Title = title };
        problem.Extensions["code"] = code;
        http.Response.StatusCode = status;

        return http.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = http, ProblemDetails = problem })
            .AsTask();
    }
}
