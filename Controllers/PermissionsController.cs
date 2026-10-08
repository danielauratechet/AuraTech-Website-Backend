using AuratechBackend.Constants;
using AuratechBackend.Services.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers;

[Authorize]
[Route(ApiRoutes.Prefix + "/permissions")]
public sealed class PermissionsController : BaseApiController
{
    private readonly IRoleService _roleService;

    public PermissionsController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => FromResult(await _roleService.GetPermissionsAsync(cancellationToken));
}
