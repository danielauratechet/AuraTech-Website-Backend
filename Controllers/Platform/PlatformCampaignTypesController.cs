using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Services.CampaignTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Platform;

[Authorize(Policy = AuthPolicies.PlatformOrganization)]
[Route(ApiRoutes.Prefix + "/platform/campaign-types")]
public sealed class PlatformCampaignTypesController : BaseApiController
{
    private readonly ICampaignTypeService _campaignTypeService;

    public PlatformCampaignTypesController(ICampaignTypeService campaignTypeService)
    {
        _campaignTypeService = campaignTypeService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.CampaignTypesRead)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
        => FromResult(await _campaignTypeService.ListAsync(activeOnly: !includeInactive, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.CampaignTypesRead)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.GetByIdAsync(id, activeOnly: false, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthPolicies.CampaignTypesManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCampaignTypeRequest request,
        CancellationToken cancellationToken)
        => FromResult(
            await _campaignTypeService.CreateAsync(request, cancellationToken),
            StatusCodes.Status201Created);

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.CampaignTypesManage)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCampaignTypeRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Policy = AuthPolicies.CampaignTypesManage)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.SetActiveAsync(id, isActive: false, cancellationToken));

    [HttpPost("{id:int}/activate")]
    [Authorize(Policy = AuthPolicies.CampaignTypesManage)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignTypeService.SetActiveAsync(id, isActive: true, cancellationToken));
}
