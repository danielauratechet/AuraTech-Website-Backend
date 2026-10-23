using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Campaigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/analytics")]
public sealed class CustomerCampaignAnalyticsController : BaseApiController
{
    private readonly ICampaignAnalyticsService _analyticsService;

    public CustomerCampaignAnalyticsController(ICampaignAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("overview")]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> Overview(
        [FromQuery] CampaignAnalyticsOverviewQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _analyticsService.GetOverviewAsync(
            User.GetOrganizationId(),
            query,
            cancellationToken));
}
