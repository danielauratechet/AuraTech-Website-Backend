using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.ConversionTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/conversion-tracking")]
public sealed class CustomerConversionTrackingController : BaseApiController
{
    private readonly IConversionTrackingService _conversionTrackingService;

    public CustomerConversionTrackingController(IConversionTrackingService conversionTrackingService)
    {
        _conversionTrackingService = conversionTrackingService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => FromResult(await _conversionTrackingService.GetAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPut]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertConversionTrackingRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _conversionTrackingService.UpsertAsync(
            User.GetOrganizationId(),
            request,
            cancellationToken));
}
