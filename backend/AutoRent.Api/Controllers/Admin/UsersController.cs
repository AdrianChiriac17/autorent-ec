using AutoRent.Api.Extensions;
using AutoRent.Application.Abstractions;
using AutoRent.Application.Common;
using AutoRent.Application.DTOs.Admin;
using AutoRent.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoRent.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = AccessPolicies.Admin)]
[Produces("application/json")]
public sealed class UsersController(UserAdminService userAdminService, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<AdminUserDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<AdminUserDto>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
        CancellationToken ct = default) =>
        userAdminService.ListUsersAsync(page, pageSize, search, ct);

    [HttpPatch("{id:long}")]
    [ProducesResponseType<AdminUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<AdminUserDto> UpdateAccess(long id, UpdateUserAccessRequest request, CancellationToken ct) =>
        userAdminService.UpdateAccessAsync(currentUser.RequiredUserId, id, request, ct);
}
