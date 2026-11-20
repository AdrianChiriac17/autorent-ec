using AutoRent.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Application.Services;

/// <summary>
/// Reads a user's current access flags from the database, so suspension and admin
/// demotion take effect immediately instead of when an old JWT expires.
/// </summary>
public sealed class UserAccessService(IAutoRentDbContextProvider dbProvider)
{
    public async Task<UserAccess?> GetAccessAsync(long userId, CancellationToken ct)
    {
        await using var db = await dbProvider.CreateDbContextAsync(ct);

        return await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserAccess(u.IsAccountActive, u.IsAdmin))
            .SingleOrDefaultAsync(ct);
    }
}

public sealed record UserAccess(bool IsAccountActive, bool IsAdmin);
