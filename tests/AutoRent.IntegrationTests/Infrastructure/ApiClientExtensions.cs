using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoRent.Application.DTOs.Auth;

namespace AutoRent.IntegrationTests.Infrastructure;

public static class ApiClientExtensions
{
    public const string AdminEmail = "admin@autorent.local";

    public static async Task<AuthResponse> LoginAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static async Task<AuthResponse> RegisterAsync(this HttpClient client, string email, string? password = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = password ?? TestPassword, fullName = "Test User" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Reads the stable machine-readable "code" from a ProblemDetails response.</summary>
    public static async Task<string?> ReadProblemCodeAsync(this HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    /// <summary>Password used for accounts the tests register themselves; random per run.</summary>
    public static string TestPassword { get; } = $"T-{Guid.NewGuid():N}";

    /// <summary>A unique email per test so tests never collide on the shared database.</summary>
    public static string UniqueEmail(string prefix = "user") => $"{prefix}.{Guid.NewGuid():N}@example.com";
}
