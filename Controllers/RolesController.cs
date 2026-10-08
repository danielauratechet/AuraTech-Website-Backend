using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers;

[Authorize]
[Route(ApiRoutes.Prefix + "/roles")]
public sealed class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("predefined")]
    public IActionResult GetPredefined()
    {
        var orgType = User.GetOrganizationType();
        if (orgType is null)
        {
            return Unauthorized();
        }

        return FromResult(_roleService.GetPredefinedRoles(orgType.Value));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => FromResult(await _roleService.GetRolesAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthPolicies.RolesManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
        => FromResult(await _roleService.CreateRoleAsync(User.GetOrganizationId(), request, cancellationToken), StatusCodes.Status201Created);
}
