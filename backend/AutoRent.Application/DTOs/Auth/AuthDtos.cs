using System.ComponentModel.DataAnnotations;

namespace AutoRent.Application.DTOs.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256), Display(Name = "Email")] string Email,
    // BCrypt only uses the first 72 bytes of a password, so longer ones are rejected.
    [Required, MinLength(8), MaxLength(72), Display(Name = "Password")] string Password,
    [Required, MinLength(2), MaxLength(150), Display(Name = "Full name")] string FullName,
    [Phone, MaxLength(30), Display(Name = "Phone")] string? Phone);

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256), Display(Name = "Email")] string Email,
    [Required, MaxLength(72)] string Password);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserProfileDto User);

public sealed record UserProfileDto(
    long Id,
    string Email,
    string FullName,
    string? Phone,
    bool IsAdmin,
    string? AddressLine,
    string? City,
    string? County,
    string? PostalCode,
    string? Country);
