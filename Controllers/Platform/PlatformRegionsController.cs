using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Services.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Platform;

[Authorize(Policy = AuthPolicies.PlatformOrganization)]
[Route(ApiRoutes.Prefix + "/platform/regions")]
public sealed class PlatformRegionsController : BaseApiController
{
    private readonly IRegionService _regionService;

    public PlatformRegionsController(IRegionService regionService)
    {
        _regionService = regionService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.RegionsRead)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
        => FromResult(await _regionService.ListAsync(activeOnly: !includeInactive, search: q, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.RegionsRead)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => FromResult(await _regionService.GetByIdAsync(id, activeOnly: false, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthPolicies.RegionsManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRegionRequest request,
        CancellationToken cancellationToken)
        => FromResult(
            await _regionService.CreateAsync(request, cancellationToken),
            StatusCodes.Status201Created);

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.RegionsManage)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRegionRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _regionService.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Policy = AuthPolicies.RegionsManage)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        => FromResult(await _regionService.SetActiveAsync(id, isActive: false, cancellationToken));

    [HttpPost("{id:int}/activate")]
    [Authorize(Policy = AuthPolicies.RegionsManage)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => FromResult(await _regionService.SetActiveAsync(id, isActive: true, cancellationToken));
}
