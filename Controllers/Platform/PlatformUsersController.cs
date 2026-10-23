using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Platform;

[Authorize(Policy = AuthPolicies.PlatformOrganization)]
[Authorize(Policy = AuthPolicies.UsersManage)]
[Route(ApiRoutes.Prefix + "/platform/users")]
public sealed class PlatformUsersController : BaseApiController
{
    private readonly IOrganizationUserService _userService;

    public PlatformUsersController(IOrganizationUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => FromResult(await _userService.ListAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationUserRequest request, CancellationToken cancellationToken)
        => FromResult(await _userService.CreateAsync(User.GetOrganizationId(), User.GetUserId(), request, cancellationToken), StatusCodes.Status201Created);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationUserRequest request, CancellationToken cancellationToken)
        => FromResult(await _userService.UpdateAsync(User.GetOrganizationId(), User.GetUserId(), id, request, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        => FromResult(await _userService.DeactivateAsync(User.GetOrganizationId(), User.GetUserId(), id, cancellationToken));

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => FromResult(await _userService.ActivateAsync(User.GetOrganizationId(), User.GetUserId(), id, cancellationToken));
}
