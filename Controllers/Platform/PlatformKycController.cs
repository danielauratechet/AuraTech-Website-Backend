using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Kyc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Platform;

[Authorize(Policy = AuthPolicies.PlatformOrganization)]
[Route(ApiRoutes.Prefix + "/platform/kyc")]
public sealed class PlatformKycController : BaseApiController
{
    private readonly ICustomerKycService _kycService;

    public PlatformKycController(ICustomerKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.KycView)]
    public async Task<IActionResult> List([FromQuery] PlatformKycListQuery query, CancellationToken cancellationToken)
        => FromResult(await _kycService.ListAsync(query, cancellationToken));

    [HttpGet("{organizationId:int}")]
    [Authorize(Policy = AuthPolicies.KycView)]
    public async Task<IActionResult> Get(int organizationId, CancellationToken cancellationToken)
        => FromResult(await _kycService.GetByOrganizationIdAsync(organizationId, cancellationToken));

    [HttpPost("{organizationId:int}/approve")]
    [Authorize(Policy = AuthPolicies.KycApprove)]
    public async Task<IActionResult> Approve(int organizationId, CancellationToken cancellationToken)
        => FromResult(await _kycService.ApproveAsync(organizationId, User.GetUserId(), cancellationToken));

    [HttpPost("{organizationId:int}/reject")]
    [Authorize(Policy = AuthPolicies.KycReject)]
    public async Task<IActionResult> Reject(
        int organizationId,
        [FromBody] RejectCustomerKycRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _kycService.RejectAsync(organizationId, User.GetUserId(), request, cancellationToken));
}
