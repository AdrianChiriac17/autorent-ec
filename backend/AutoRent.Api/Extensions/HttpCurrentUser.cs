using System.Globalization;
using AutoRent.Application.Abstractions;
using AutoRent.Application.Common;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AutoRent.Api.Extensions;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public long? UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return long.TryParse(sub, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
        }
    }

    public long RequiredUserId =>
        UserId ?? throw AppException.Unauthorized("auth.unauthenticated", "Authentication is required.");
}
