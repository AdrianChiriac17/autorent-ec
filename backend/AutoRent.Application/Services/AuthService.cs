using AutoRent.Application.Abstractions;
using AutoRent.Application.Common;
using AutoRent.Application.DTOs.Auth;
using AutoRent.Application.Validation;
using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Application.Services;

public sealed class AuthService(
    IAutoRentDbContextProvider dbProvider,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer)
{
    private const string InvalidCredentialsMessage = "Email or password is incorrect.";

    // Verified against when the email is unknown, so a login for a missing account
    // takes as long as one with a wrong password (no account enumeration by timing).
    private readonly Lazy<string> _dummyHash = new(() => passwordHasher.Hash("autorent-timing-equaliser"));

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        await using var db = await dbProvider.CreateDbContextAsync(ct);

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            throw AppException.Conflict("auth.email_taken", "An account with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            IsAdmin = false,
            IsAccountActive = true,
        };
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Lost a race with a concurrent registration: the unique index on Email rejected the insert.
            throw AppException.Conflict("auth.email_taken", "An account with this email already exists.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        await using var db = await dbProvider.CreateDbContextAsync(ct);
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            passwordHasher.Verify(request.Password, _dummyHash.Value);
            throw AppException.Unauthorized("auth.invalid_credentials", InvalidCredentialsMessage);
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw AppException.Unauthorized("auth.invalid_credentials", InvalidCredentialsMessage);
        }

        // Only revealed after a correct password, so it does not leak which emails exist.
        if (!user.IsAccountActive)
        {
            throw AppException.Forbidden("auth.account_suspended", "This account has been suspended.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserProfileDto> GetProfileAsync(long userId, CancellationToken ct)
    {
        await using var db = await dbProvider.CreateDbContextAsync(ct);

        return await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileDto(
                u.Id, u.Email, u.FullName, u.Phone, u.IsAdmin,
                u.AddressLine, u.City, u.County, u.PostalCode, u.Country))
            .SingleOrDefaultAsync(ct)
            ?? throw AppException.NotFound("user.not_found", "User not found.");
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = tokenIssuer.Issue(user);
        return new AuthResponse(token.Token, token.ExpiresAt, ToProfile(user));
    }

    private static UserProfileDto ToProfile(User u) => new(
        u.Id, u.Email, u.FullName, u.Phone, u.IsAdmin,
        u.AddressLine, u.City, u.County, u.PostalCode, u.Country);
}
