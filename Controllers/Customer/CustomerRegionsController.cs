using AuratechBackend.Constants;
using AuratechBackend.Services.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/regions")]
public sealed class CustomerRegionsController : BaseApiController
{
    private readonly IRegionService _regionService;

    public CustomerRegionsController(IRegionService regionService)
    {
        _regionService = regionService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.RegionsRead)]
    public async Task<IActionResult> List(
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
        => FromResult(await _regionService.ListAsync(activeOnly: true, search: q, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.RegionsRead)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => FromResult(await _regionService.GetByIdAsync(id, activeOnly: true, cancellationToken));
}
