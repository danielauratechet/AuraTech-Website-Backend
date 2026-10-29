using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Campaigns;
using AuratechBackend.Services.ConversionTracking;
using AuratechBackend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/campaigns")]
public sealed class CustomerCampaignsController : BaseApiController
{
    private readonly ICampaignService _campaignService;
    private readonly IConversionTrackingService _conversionTrackingService;
    private readonly ISupabaseStorageService _supabaseStorage;
    private readonly ILogger<CustomerCampaignsController> _logger;

    public CustomerCampaignsController(
        ICampaignService campaignService,
        IConversionTrackingService conversionTrackingService,
        ISupabaseStorageService supabaseStorage,
        ILogger<CustomerCampaignsController> logger)
    {
        _campaignService = campaignService;
        _conversionTrackingService = conversionTrackingService;
        _supabaseStorage = supabaseStorage;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> Create(
        [FromBody] SaveCampaignStep1Request request,
        CancellationToken cancellationToken)
        => FromResult(
            await _campaignService.CreateAsync(
                User.GetOrganizationId(),
                User.GetUserId(),
                request,
                cancellationToken),
            StatusCodes.Status201Created);

    [HttpGet]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> List(
        [FromQuery] CustomerCampaignListQuery query,
        CancellationToken cancellationToken = default)
        => FromResult(await _campaignService.ListAsync(
            User.GetOrganizationId(),
            query,
            cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignService.GetByIdAsync(User.GetOrganizationId(), id, cancellationToken));

    [HttpPut("{id:int}/steps/1")]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> SaveStep1(
        int id,
        [FromBody] SaveCampaignStep1Request request,
        CancellationToken cancellationToken)
        => FromResult(await _campaignService.SaveStep1Async(
            User.GetOrganizationId(),
            User.GetUserId(),
            id,
            request,
            cancellationToken));

    [HttpPut("{id:int}/steps/2")]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> SaveStep2(
        int id,
        [FromBody] SaveCampaignStep2Request request,
        CancellationToken cancellationToken)
        => FromResult(await _campaignService.SaveStep2Async(
            User.GetOrganizationId(),
            User.GetUserId(),
            id,
            request,
            cancellationToken));

    [HttpPut("{id:int}/steps/3")]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> SaveStep3(
        int id,
        [FromBody] SaveCampaignStep3Request request,
        CancellationToken cancellationToken)
        => FromResult(await _campaignService.SaveStep3Async(
            User.GetOrganizationId(),
            User.GetUserId(),
            id,
            request,
            cancellationToken));

    [HttpGet("{id:int}/summary")]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> Summary(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignService.GetSummaryAsync(User.GetOrganizationId(), id, cancellationToken));

    [HttpPost("{id:int}/presign-creative")]
    [Authorize(Policy = AuthPolicies.CampaignsPresign)]
    public async Task<IActionResult> PresignCreative(
        int id,
        [FromBody] PresignCampaignCreativeRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = User.GetOrganizationId();
        var campaignResult = await _campaignService.GetByIdAsync(organizationId, id, cancellationToken);
        if (campaignResult.IsFailure)
        {
            return FromResult(campaignResult);
        }

        if (campaignResult.Value.Status != CampaignStatus.Draft)
        {
            return FromResult(Result<PresignCampaignCreativeResponse>.Conflict(
                "Creative upload is only allowed for draft campaigns."));
        }

        var safeFileName = Path.GetFileName(request.FileName.Trim());
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            return FromResult(Result<PresignCampaignCreativeResponse>.ValidationFailure(
            [
                new ValidationError(nameof(request.FileName), "FileName is required.")
            ]));
        }

        var objectKey = $"campaigns/{organizationId}/{id}/{DateTime.UtcNow.Ticks}_{safeFileName}";

        try
        {
            var (uploadUrl, publicUrl) = await _supabaseStorage.CreateSignedUploadUrlAsync(objectKey, safeFileName);
            return FromResult(Result<PresignCampaignCreativeResponse>.Success(new PresignCampaignCreativeResponse
            {
                UploadUrl = uploadUrl,
                PublicUrl = publicUrl,
                ObjectKey = objectKey
            }, "Signed upload URL created."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to presign campaign creative for org {OrganizationId} campaign {CampaignId}", organizationId, id);
            return FromResult(Result<PresignCampaignCreativeResponse>.Failure(
                ErrorCodes.Unexpected,
                "Failed to create signed upload URL.",
                StatusCodes.Status500InternalServerError));
        }
    }

    [HttpGet("{id:int}/conversion-analytics")]
    [Authorize(Policy = AuthPolicies.CampaignsRead)]
    public async Task<IActionResult> ConversionAnalytics(
        int id,
        [FromQuery] CampaignConversionAnalyticsQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _conversionTrackingService.GetCampaignAnalyticsAsync(
            User.GetOrganizationId(),
            id,
            query,
            cancellationToken));

    [HttpPost("{id:int}/launch")]
    [Authorize(Policy = AuthPolicies.CampaignsLaunch)]
    public async Task<IActionResult> Launch(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignService.LaunchAsync(User.GetOrganizationId(), User.GetUserId(), id, cancellationToken));

    [HttpPost("{id:int}/pause")]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignService.PauseAsync(User.GetOrganizationId(), User.GetUserId(), id, cancellationToken));

    [HttpPost("{id:int}/resume")]
    [Authorize(Policy = AuthPolicies.CampaignsWrite)]
    public async Task<IActionResult> Resume(int id, CancellationToken cancellationToken)
        => FromResult(await _campaignService.ResumeAsync(User.GetOrganizationId(), User.GetUserId(), id, cancellationToken));
}
