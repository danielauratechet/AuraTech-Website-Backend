using AuratechBackend.Constants;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/activity")]
public sealed class CustomerActivityController : BaseApiController
{
    private readonly IRecentActivityService _activityService;

    public CustomerActivityController(IRecentActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 5, CancellationToken cancellationToken = default)
        => FromResult(await _activityService.ListAsync(User.GetOrganizationId(), limit, cancellationToken));
}
