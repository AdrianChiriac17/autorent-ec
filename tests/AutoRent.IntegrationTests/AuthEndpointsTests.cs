using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoRent.Application.DTOs.Auth;
using AutoRent.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static AutoRent.IntegrationTests.Infrastructure.ApiClientExtensions;

namespace AutoRent.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class AuthEndpointsTests(ApiFixture api)
{
    [Fact]
    public async Task Register_stores_a_bcrypt_hash_and_returns_a_working_token()
    {
        var client = api.CreateClient();
        var email = UniqueEmail();

        var auth = await client.RegisterAsync(email);

        Assert.Equal(email, auth.User.Email);
        Assert.False(auth.User.IsAdmin);

        await using var db = await api.CreateDbContextAsync();
        var stored = await db.Users.SingleAsync(u => u.Email == email);
        Assert.NotEqual(TestPassword, stored.PasswordHash);
        Assert.StartsWith("$2", stored.PasswordHash, StringComparison.Ordinal);

        var me = await client.WithToken(auth.AccessToken).GetFromJsonAsync<UserProfileDto>("/api/v1/auth/me");
        Assert.Equal(email, me!.Email);
    }

    [Fact]
    public async Task Register_rejects_an_existing_email_regardless_of_case()
    {
        var client = api.CreateClient();
        var email = UniqueEmail();
        await client.RegisterAsync(email);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email = email.ToUpperInvariant(), password = TestPassword, fullName = "Someone Else" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("auth.email_taken", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Register_returns_camelCase_field_errors_for_invalid_input()
    {
        var response = await api.CreateClient().PostAsJsonAsync("/api/v1/auth/register",
            new { email = "not-an-email", password = "short", fullName = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation.failed", doc.RootElement.GetProperty("code").GetString());
        var errors = doc.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("password", out _));
        Assert.True(errors.TryGetProperty("fullName", out _));
    }

    [Fact]
    public async Task Login_accepts_the_email_in_any_case()
    {
        var auth = await api.CreateClient().LoginAsync("ADMIN@AutoRent.Local", api.AdminPassword);

        Assert.True(auth.User.IsAdmin);
        Assert.True(auth.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_gives_the_same_generic_error_for_a_wrong_password_and_an_unknown_email()
    {
        var client = api.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = AdminEmail, password = "not-the-right-one" });
        var unknownEmail = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = UniqueEmail(), password = "not-the-right-one" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal("auth.invalid_credentials", await wrongPassword.ReadProblemCodeAsync());
        Assert.Equal("auth.invalid_credentials", await unknownEmail.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Suspended_account_cannot_log_in()
    {
        var client = api.CreateClient();
        var email = UniqueEmail("suspended");
        var user = await client.RegisterAsync(email);
        await SetActiveAsync(user.User.Id, false);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = TestPassword });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("auth.account_suspended", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Suspension_blocks_an_already_issued_token_immediately()
    {
        var client = api.CreateClient();
        var user = await client.RegisterAsync(UniqueEmail("revoked"));
        client.WithToken(user.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/auth/me")).StatusCode);

        await SetActiveAsync(user.User.Id, false);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
    }

    /// <summary>A well-formed token (header.payload.signature) whose signature is not ours. Built at runtime.</summary>
    private static string ForgedToken()
    {
        static string Part(string json) => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return string.Join('.', Part("""{"alg":"HS256","typ":"JWT"}"""), Part("""{"sub":"1"}"""), Part("forged"));
    }

    public static TheoryData<string?> BadTokens => new() { null, "not-a-jwt", ForgedToken() };

    [Theory]
    [MemberData(nameof(BadTokens))]
    public async Task Protected_endpoints_reject_missing_or_malformed_tokens(string? token)
    {
        var client = api.CreateClient();
        if (token is not null)
        {
            client.WithToken(token);
        }

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.unauthenticated", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Logout_returns_no_content_for_an_authenticated_user()
    {
        var client = api.CreateClient();
        var user = await client.RegisterAsync(UniqueEmail());

        var response = await client.WithToken(user.AccessToken).PostAsync("/api/v1/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task SetActiveAsync(long userId, bool isActive)
    {
        await using var db = await api.CreateDbContextAsync();
        await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(u => u.IsAccountActive, isActive));
    }
}
