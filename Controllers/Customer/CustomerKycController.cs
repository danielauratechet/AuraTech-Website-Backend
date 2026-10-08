using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Kyc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/kyc")]
public sealed class CustomerKycController : BaseApiController
{
    private readonly ICustomerKycService _kycService;

    public CustomerKycController(ICustomerKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.KycRead)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => FromResult(await _kycService.GetForOrganizationAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthPolicies.KycWrite)]
    public async Task<IActionResult> Create([FromBody] SubmitCustomerKycRequest request, CancellationToken cancellationToken)
        => FromResult(
            await _kycService.CreateAsync(User.GetOrganizationId(), request, cancellationToken),
            StatusCodes.Status201Created);

    [HttpPut]
    [Authorize(Policy = AuthPolicies.KycUpdate)]
    public async Task<IActionResult> Update([FromBody] SubmitCustomerKycRequest request, CancellationToken cancellationToken)
        => FromResult(await _kycService.UpdateAsync(User.GetOrganizationId(), request, cancellationToken));
}
