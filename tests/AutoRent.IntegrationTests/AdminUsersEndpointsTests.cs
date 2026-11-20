using System.Net;
using System.Net.Http.Json;
using AutoRent.Application.Common;
using AutoRent.Application.DTOs.Admin;
using AutoRent.IntegrationTests.Infrastructure;
using static AutoRent.IntegrationTests.Infrastructure.ApiClientExtensions;

namespace AutoRent.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class AdminUsersEndpointsTests(ApiFixture api)
{
    private const string UsersUrl = "/api/v1/admin/users";

    [Fact]
    public async Task Anonymous_callers_get_401()
    {
        var response = await api.CreateClient().GetAsync(UsersUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Regular_users_are_blocked_on_the_server_with_403()
    {
        var client = api.CreateClient();
        var user = await client.RegisterAsync(UniqueEmail());
        client.WithToken(user.AccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(UsersUrl)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PatchAsJsonAsync($"{UsersUrl}/{user.User.Id}", new { isAdmin = true })).StatusCode);
    }

    [Fact]
    public async Task Admin_can_list_users_with_pagination_metadata()
    {
        var client = await AdminClientAsync();

        var page = await client.GetFromJsonAsync<PagedResult<AdminUserDto>>($"{UsersUrl}?page=1&pageSize=2");

        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.True(page.TotalCount >= 3);
        Assert.Contains(page.Items, u => u.Email == AdminEmail);
    }

    [Fact]
    public async Task Admin_can_search_users_by_email()
    {
        var client = await AdminClientAsync();

        var page = await client.GetFromJsonAsync<PagedResult<AdminUserDto>>($"{UsersUrl}?search=ana.popescu");

        var user = Assert.Single(page!.Items);
        Assert.Equal("ana.popescu@autorent.local", user.Email);
    }

    [Fact]
    public async Task Promotion_takes_effect_immediately_for_the_promoted_user()
    {
        var admin = await AdminClientAsync();
        var userClient = api.CreateClient();
        var user = await userClient.RegisterAsync(UniqueEmail("promoted"));
        userClient.WithToken(user.AccessToken);
        Assert.Equal(HttpStatusCode.Forbidden, (await userClient.GetAsync(UsersUrl)).StatusCode);

        var response = await admin.PatchAsJsonAsync($"{UsersUrl}/{user.User.Id}", new { isAdmin = true });

        response.EnsureSuccessStatusCode();
        Assert.True((await response.Content.ReadFromJsonAsync<AdminUserDto>())!.IsAdmin);
        Assert.Equal(HttpStatusCode.OK, (await userClient.GetAsync(UsersUrl)).StatusCode);
    }

    [Fact]
    public async Task Admin_can_suspend_and_reactivate_a_user()
    {
        var admin = await AdminClientAsync();
        var user = await api.CreateClient().RegisterAsync(UniqueEmail("toggled"));

        var suspended = await admin.PatchAsJsonAsync($"{UsersUrl}/{user.User.Id}", new { isAccountActive = false });
        var reactivated = await admin.PatchAsJsonAsync($"{UsersUrl}/{user.User.Id}", new { isAccountActive = true });

        Assert.False((await suspended.Content.ReadFromJsonAsync<AdminUserDto>())!.IsAccountActive);
        Assert.True((await reactivated.Content.ReadFromJsonAsync<AdminUserDto>())!.IsAccountActive);
    }

    [Fact]
    public async Task Admin_cannot_demote_or_suspend_themselves()
    {
        var client = api.CreateClient();
        var admin = await client.LoginAsync(AdminEmail, api.AdminPassword);
        client.WithToken(admin.AccessToken);

        var demote = await client.PatchAsJsonAsync($"{UsersUrl}/{admin.User.Id}", new { isAdmin = false });
        var suspend = await client.PatchAsJsonAsync($"{UsersUrl}/{admin.User.Id}", new { isAccountActive = false });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, demote.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, suspend.StatusCode);
        Assert.Equal("admin.self_lockout", await demote.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Updating_a_missing_user_returns_404()
    {
        var admin = await AdminClientAsync();

        var response = await admin.PatchAsJsonAsync($"{UsersUrl}/999999", new { isAdmin = true });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpClient> AdminClientAsync()
    {
        var client = api.CreateClient();
        var admin = await client.LoginAsync(AdminEmail, api.AdminPassword);
        return client.WithToken(admin.AccessToken);
    }
}
