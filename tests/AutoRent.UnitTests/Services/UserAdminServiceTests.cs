using AutoRent.Application.Abstractions;
using AutoRent.Application.Common;
using AutoRent.Application.DTOs.Admin;
using AutoRent.Application.Services;
using AutoRent.Application.Validation;

namespace AutoRent.UnitTests.Services;

public sealed class UserAdminServiceTests
{
    // These rules are checked before any database access, so the provider must never be used.
    private readonly UserAdminService _service = new(new NoDatabaseProvider());

    [Theory]
    [InlineData(false, null)]
    [InlineData(null, false)]
    [InlineData(false, false)]
    public async Task An_admin_cannot_demote_or_suspend_themselves(bool? isAdmin, bool? isActive)
    {
        var error = await Assert.ThrowsAsync<AppException>(() =>
            _service.UpdateAccessAsync(7, 7, new UpdateUserAccessRequest(isAdmin, isActive), CancellationToken.None));

        Assert.Equal(ErrorKind.BusinessRule, error.Kind);
        Assert.Equal("admin.self_lockout", error.Code);
    }

    [Fact]
    public async Task An_empty_update_is_rejected()
    {
        var error = await Assert.ThrowsAsync<AppException>(() =>
            _service.UpdateAccessAsync(7, 8, new UpdateUserAccessRequest(null, null), CancellationToken.None));

        Assert.Equal(ErrorKind.Validation, error.Kind);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, UserAdminService.MaxPageSize + 1)]
    public async Task Invalid_pagination_is_rejected(int page, int pageSize)
    {
        var error = await Assert.ThrowsAsync<AppException>(() =>
            _service.ListUsersAsync(page, pageSize, null, CancellationToken.None));

        Assert.Equal("pagination.invalid", error.Code);
    }

    [Theory]
    [InlineData("  Ana.Popescu@Example.COM ", "ana.popescu@example.com")]
    [InlineData("admin@autorent.local", "admin@autorent.local")]
    public void Emails_are_normalised_to_trimmed_lower_case(string input, string expected)
    {
        Assert.Equal(expected, EmailNormalizer.Normalize(input));
    }

    [Fact]
    public void Paged_result_rounds_total_pages_up()
    {
        Assert.Equal(3, new PagedResult<int>([], 1, 20, 41).TotalPages);
    }

    private sealed class NoDatabaseProvider : IAutoRentDbContextProvider
    {
        public Task<IAutoRentDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The database must not be touched for this rule.");
    }
}
