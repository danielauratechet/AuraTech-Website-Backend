using AuratechBackend.Constants;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Kyc;
using AuratechBackend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/kyc")]
public sealed class CustomerKycController : BaseApiController
{
    private readonly ICustomerKycService _kycService;
    private readonly ISupabaseStorageService _supabaseStorage;
    private readonly ILogger<CustomerKycController> _logger;

    public CustomerKycController(
        ICustomerKycService kycService,
        ISupabaseStorageService supabaseStorage,
        ILogger<CustomerKycController> logger)
    {
        _kycService = kycService;
        _supabaseStorage = supabaseStorage;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.KycRead)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => FromResult(await _kycService.GetForOrganizationAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPost("presign")]
    [Authorize(Policy = AuthPolicies.KycPresign)]
    public async Task<IActionResult> Presign([FromBody] PresignKycUploadRequest request, CancellationToken cancellationToken)
    {
        var organizationId = User.GetOrganizationId();
        var kind = request.Kind.Trim().ToLowerInvariant();
        var safeFileName = Path.GetFileName(request.FileName.Trim());
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            return FromResult(Result<PresignKycUploadResponse>.ValidationFailure(
            [
                new ValidationError(nameof(request.FileName), "FileName is required.")
            ]));
        }

        var objectKey = $"kyc/{organizationId}/{kind}/{DateTime.UtcNow.Ticks}_{safeFileName}";

        try
        {
            var (uploadUrl, publicUrl) = await _supabaseStorage.CreateSignedUploadUrlAsync(objectKey, safeFileName);
            return FromResult(Result<PresignKycUploadResponse>.Success(new PresignKycUploadResponse
            {
                UploadUrl = uploadUrl,
                PublicUrl = publicUrl,
                ObjectKey = objectKey
            }, "Signed upload URL created."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to presign KYC upload for org {OrganizationId}", organizationId);
            return FromResult(Result<PresignKycUploadResponse>.Failure(
                ErrorCodes.Unexpected,
                "Failed to create signed upload URL.",
                StatusCodes.Status500InternalServerError));
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.KycWrite)]
    public async Task<IActionResult> Create([FromBody] SubmitCustomerKycRequest request, CancellationToken cancellationToken)
        => FromResult(
            await _kycService.CreateAsync(User.GetOrganizationId(), User.GetUserId(), request, cancellationToken),
            StatusCodes.Status201Created);

    [HttpPut]
    [Authorize(Policy = AuthPolicies.KycUpdate)]
    public async Task<IActionResult> Update([FromBody] SubmitCustomerKycRequest request, CancellationToken cancellationToken)
        => FromResult(await _kycService.UpdateAsync(User.GetOrganizationId(), User.GetUserId(), request, cancellationToken));
}
