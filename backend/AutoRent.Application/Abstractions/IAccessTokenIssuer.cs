using AutoRent.Domain.Entities;

namespace AutoRent.Application.Abstractions;

public interface IAccessTokenIssuer
{
    AccessToken Issue(User user);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
