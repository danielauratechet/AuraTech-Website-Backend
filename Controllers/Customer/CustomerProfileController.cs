using AuratechBackend.Constants;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Organizations;
using AuratechBackend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/profile")]
public sealed class CustomerProfileController : BaseApiController
{
    private readonly IOrganizationProfileService _profileService;
    private readonly ISupabaseStorageService _supabaseStorage;
    private readonly ILogger<CustomerProfileController> _logger;

    public CustomerProfileController(
        IOrganizationProfileService profileService,
        ISupabaseStorageService supabaseStorage,
        ILogger<CustomerProfileController> logger)
    {
        _profileService = profileService;
        _supabaseStorage = supabaseStorage;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => FromResult(await _profileService.GetAsync(User.GetOrganizationId(), cancellationToken));

    [HttpPut]
    [Authorize(Policy = AuthPolicies.OrgManage)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateOrganizationProfileRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _profileService.UpdateAsync(
            User.GetOrganizationId(),
            User.GetUserId(),
            request,
            cancellationToken));

    [HttpPost("presign-logo")]
    [Authorize(Policy = AuthPolicies.OrgManage)]
    public async Task<IActionResult> PresignLogo(
        [FromBody] PresignProfileLogoRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = User.GetOrganizationId();
        var safeFileName = Path.GetFileName(request.FileName.Trim());
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            return FromResult(Result<PresignProfileLogoResponse>.ValidationFailure(
            [
                new ValidationError(nameof(request.FileName), "FileName is required.")
            ]));
        }

        var objectKey = $"profiles/{organizationId}/logo/{DateTime.UtcNow.Ticks}_{safeFileName}";

        try
        {
            var (uploadUrl, publicUrl) = await _supabaseStorage.CreateSignedUploadUrlAsync(objectKey, safeFileName);
            return FromResult(Result<PresignProfileLogoResponse>.Success(new PresignProfileLogoResponse
            {
                UploadUrl = uploadUrl,
                PublicUrl = publicUrl,
                ObjectKey = objectKey
            }, "Signed upload URL created."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to presign profile logo for org {OrganizationId}", organizationId);
            return FromResult(Result<PresignProfileLogoResponse>.Failure(
                ErrorCodes.Unexpected,
                "Failed to create signed upload URL.",
                StatusCodes.Status500InternalServerError));
        }
    }
}
