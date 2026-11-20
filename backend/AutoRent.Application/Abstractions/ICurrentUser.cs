namespace AutoRent.Application.Abstractions;

/// <summary>The authenticated caller, as identified by the validated access token.</summary>
public interface ICurrentUser
{
    /// <summary>The user ID from the token, or null for anonymous requests.</summary>
    long? UserId { get; }

    /// <summary>The user ID, or an Unauthorized error when the request is anonymous.</summary>
    long RequiredUserId { get; }
}
