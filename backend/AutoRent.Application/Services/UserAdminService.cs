using AutoRent.Application.Abstractions;
using AutoRent.Application.Common;
using AutoRent.Application.DTOs.Admin;
using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Application.Services;

public sealed class UserAdminService(IAutoRentDbContextProvider dbProvider)
{
    public const int MaxPageSize = 100;

    public async Task<PagedResult<AdminUserDto>> ListUsersAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        if (page < 1 || pageSize < 1 || pageSize > MaxPageSize)
        {
            throw AppException.Validation(
                "pagination.invalid", $"Page must be at least 1 and page size between 1 and {MaxPageSize}.");
        }

        await using var db = await dbProvider.CreateDbContextAsync(ct);

        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            // Runs in SQL as lower("FullName") LIKE ...; culture and StringComparison overloads do not apply there
            // (EF Core cannot translate them). Emails are already stored lower-case.
#pragma warning disable CA1304, CA1311, CA1862
            query = query.Where(u => u.Email.Contains(term) || u.FullName.ToLower().Contains(term));
#pragma warning restore CA1304, CA1311, CA1862
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.FullName, u.Phone, u.City, u.IsAdmin, u.IsAccountActive))
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>(items, page, pageSize, total);
    }

    public async Task<AdminUserDto> UpdateAccessAsync(
        long actingAdminId, long userId, UpdateUserAccessRequest request, CancellationToken ct)
    {
        if (request.IsAdmin is null && request.IsAccountActive is null)
        {
            throw AppException.Validation("admin.empty_update", "Nothing to update: set isAdmin and/or isAccountActive.");
        }

        // An admin may not lock themselves out. This also guarantees at least one active admin remains.
        if (actingAdminId == userId && (request.IsAdmin == false || request.IsAccountActive == false))
        {
            throw AppException.BusinessRule(
                "admin.self_lockout", "You cannot remove your own admin rights or suspend your own account.");
        }

        await using var db = await dbProvider.CreateDbContextAsync(ct);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw AppException.NotFound("user.not_found", "User not found.");

        if (request.IsAdmin is { } isAdmin)
        {
            user.IsAdmin = isAdmin;
        }

        if (request.IsAccountActive is { } isActive)
        {
            user.IsAccountActive = isActive;
        }

        await db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    private static AdminUserDto ToDto(User u) =>
        new(u.Id, u.Email, u.FullName, u.Phone, u.City, u.IsAdmin, u.IsAccountActive);
}
