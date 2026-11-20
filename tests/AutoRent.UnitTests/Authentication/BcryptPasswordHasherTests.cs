using AutoRent.Infrastructure.Authentication;

namespace AutoRent.UnitTests.Authentication;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();
    private readonly string _password = $"T-{Guid.NewGuid():N}";

    [Fact]
    public void Hash_never_returns_the_plain_password_and_is_salted()
    {
        var first = _hasher.Hash(_password);
        var second = _hasher.Hash(_password);

        Assert.NotEqual(_password, first);
        Assert.StartsWith("$2", first, StringComparison.Ordinal);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_accepts_the_right_password_and_rejects_a_wrong_one()
    {
        var hash = _hasher.Hash(_password);

        Assert.True(_hasher.Verify(_password, hash));
        Assert.False(_hasher.Verify(_password + "x", hash));
    }

    [Fact]
    public void Verify_returns_false_for_a_malformed_stored_hash()
    {
        Assert.False(_hasher.Verify(_password, "not-a-bcrypt-hash"));
    }
}
