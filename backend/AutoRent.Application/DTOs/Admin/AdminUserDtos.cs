namespace AutoRent.Application.DTOs.Admin;

public sealed record AdminUserDto(
    long Id,
    string Email,
    string FullName,
    string? Phone,
    string? City,
    bool IsAdmin,
    bool IsAccountActive);

/// <summary>Partial update: only the fields that are set are changed.</summary>
public sealed record UpdateUserAccessRequest(bool? IsAdmin, bool? IsAccountActive);
