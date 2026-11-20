using AutoRent.Application.Abstractions;
using AutoRent.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace AutoRent.Api.Extensions;

public static class AccessPolicies
{
    /// <summary>Authenticated and not suspended. This is the default for [Authorize].</summary>
    public const string ActiveUser = nameof(ActiveUser);

    /// <summary>Active user whose IsAdmin flag is currently true in the database.</summary>
    public const string Admin = nameof(Admin);
}

/// <summary>Requires a valid token whose user is active (and, optionally, an admin) according to the database.</summary>
public sealed class AccountAccessRequirement(bool requireAdmin) : IAuthorizationRequirement
{
    public bool RequireAdmin { get; } = requireAdmin;
}

/// <summary>
/// Checks the current database state on every request instead of trusting token claims,
/// so suspending or demoting a user takes effect immediately, even for unexpired JWTs.
/// </summary>
public sealed class AccountAccessHandler(ICurrentUser currentUser, UserAccessService accessService, IHttpContextAccessor http)
    : AuthorizationHandler<AccountAccessRequirement>
{
    private const string CacheKey = "AutoRent.UserAccess";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, AccountAccessRequirement requirement)
    {
        if (currentUser.UserId is not { } userId)
        {
            return;
        }

        var httpContext = http.HttpContext;
        var access = httpContext?.Items[CacheKey] as UserAccess;
        if (access is null)
        {
            access = await accessService.GetAccessAsync(userId, httpContext?.RequestAborted ?? CancellationToken.None);
            if (httpContext is not null && access is not null)
            {
                httpContext.Items[CacheKey] = access;
            }
        }

        if (access is { IsAccountActive: true } && (!requirement.RequireAdmin || access.IsAdmin))
        {
            context.Succeed(requirement);
        }
    }
}
