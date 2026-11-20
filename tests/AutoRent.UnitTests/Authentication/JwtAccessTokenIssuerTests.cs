using AutoRent.Domain.Entities;
using AutoRent.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AutoRent.UnitTests.Authentication;

public sealed class JwtAccessTokenIssuerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 10, 0, 0, TimeSpan.Zero);

    private static readonly JwtOptions Options = new()
    {
        Issuer = "autorent-api",
        Audience = "autorent-web",
        SigningKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
        AccessTokenMinutes = 30,
    };

    private readonly JwtAccessTokenIssuer _issuer = new(Microsoft.Extensions.Options.Options.Create(Options), new FixedTimeProvider(Now));

    [Fact]
    public void Issued_token_expires_after_the_configured_lifetime()
    {
        var token = _issuer.Issue(new User { Id = 42, Email = "ana@example.com", FullName = "Ana" });

        Assert.Equal(Now.AddMinutes(30), token.ExpiresAt);
    }

    [Fact]
    public async Task Issued_token_is_signed_and_carries_identity_but_no_role_claims()
    {
        var token = _issuer.Issue(new User { Id = 42, Email = "ana@example.com", FullName = "Ana", IsAdmin = true });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = JwtAccessTokenIssuer.CreateSigningKey(Options.SigningKey),
            ValidateLifetime = false,
        });

        Assert.True(result.IsValid);
        Assert.Equal("42", result.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal("ana@example.com", result.Claims[JwtRegisteredClaimNames.Email]);
        // Admin rights are read from the database on each request, never from the token.
        Assert.DoesNotContain(result.Claims.Keys, k => k.Contains("admin", StringComparison.OrdinalIgnoreCase) || k.Contains("role", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var token = _issuer.Issue(new User { Id = 1, Email = "a@example.com", FullName = "A" });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = JwtAccessTokenIssuer.CreateSigningKey(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")),
            ValidateLifetime = false,
        });

        Assert.False(result.IsValid);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
