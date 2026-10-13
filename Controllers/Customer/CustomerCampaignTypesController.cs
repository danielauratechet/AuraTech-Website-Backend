using AuratechBackend.Constants;
using AuratechBackend.Services.CampaignTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/campaign-types")]
public sealed class CustomerCampaignTypesController : BaseApiController
{
    private readonly ICampaignTypeService _campaignTypeService;

    public CustomerCampaignTypesController(ICampaignTypeService campaignTypeService)
    {
        _campaignTypeService = campaignTypeService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.CampaignTypesRead)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.ListAsync(activeOnly: true, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.CampaignTypesRead)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.GetByIdAsync(id, activeOnly: true, cancellationToken));
}
