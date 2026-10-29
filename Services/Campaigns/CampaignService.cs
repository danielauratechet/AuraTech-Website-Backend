using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Dtos.Validators;
using AuratechBackend.Data.Entities;
using AuratechBackend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Campaigns;

public sealed class CampaignService : ICampaignService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public CampaignService(AppDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<Result<CampaignResponse>> CreateAsync(
        int organizationId,
        int userId,
        SaveCampaignStep1Request request,
        CancellationToken cancellationToken = default)
    {
        var typeResult = await GetActiveCampaignTypeAsync(request.CampaignTypeId, cancellationToken);
        if (typeResult.IsFailure)
        {
            return MapFailure<CampaignResponse>(typeResult);
        }

        var campaignType = typeResult.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var campaign = new Campaign
        {
            OrganizationId = organizationId,
            CreatedByUserId = userId,
            CampaignTypeId = campaignType.Id,
            Name = $"{campaignType.NameEn} – {today:yyyy-MM-dd}",
            Status = CampaignStatus.Draft,
            CurrentStep = 1,
            Languages = [],
            AgeRanges = [],
            Interests = [],
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Campaign draft created successfully.");
    }

    public async Task<Result<PagedResult<CampaignResponse>>> ListAsync(
        int organizationId,
        CustomerCampaignListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var dbQuery = _db.Campaigns.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId);

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.Status == query.Status.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var ids = await dbQuery
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var items = new List<CampaignResponse>(ids.Count);
        foreach (var id in ids)
        {
            items.Add(await MapResponseAsync(id, organizationId, query.From, query.To, cancellationToken));
        }

        return Result<PagedResult<CampaignResponse>>.Success(new PagedResult<CampaignResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }, "Campaigns retrieved successfully.");
    }

    public async Task<Result<CampaignResponse>> GetByIdAsync(
        int organizationId,
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.Campaigns.AsNoTracking()
            .AnyAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (!exists)
        {
            return Result<CampaignResponse>.NotFound("Campaign not found.");
        }

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaignId, organizationId, null, null, cancellationToken),
            "Campaign retrieved successfully.");
    }

    public async Task<Result<CampaignResponse>> SaveStep1Async(
        int organizationId,
        int actorUserId,
        int campaignId,
        SaveCampaignStep1Request request,
        CancellationToken cancellationToken = default)
    {
        var campaignResult = await GetDraftCampaignAsync(organizationId, campaignId, cancellationToken);
        if (campaignResult.IsFailure)
        {
            return MapFailure<CampaignResponse>(campaignResult);
        }

        var typeResult = await GetActiveCampaignTypeAsync(request.CampaignTypeId, cancellationToken);
        if (typeResult.IsFailure)
        {
            return MapFailure<CampaignResponse>(typeResult);
        }

        var campaign = campaignResult.Value;
        var campaignType = typeResult.Value;
        var createdDate = DateOnly.FromDateTime(campaign.CreatedAtUtc.UtcDateTime);

        campaign.CampaignTypeId = campaignType.Id;
        campaign.Name = $"{campaignType.NameEn} – {createdDate:yyyy-MM-dd}";
        campaign.CurrentStep = Math.Max(campaign.CurrentStep, 1);
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignUpdated,
            $"updated \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Step 1 saved successfully.");
    }

    public async Task<Result<CampaignResponse>> SaveStep2Async(
        int organizationId,
        int actorUserId,
        int campaignId,
        SaveCampaignStep2Request request,
        CancellationToken cancellationToken = default)
    {
        var campaignResult = await GetDraftCampaignAsync(organizationId, campaignId, cancellationToken);
        if (campaignResult.IsFailure)
        {
            return MapFailure<CampaignResponse>(campaignResult);
        }

        var campaign = campaignResult.Value;
        if (campaign.CurrentStep < 1)
        {
            return Result<CampaignResponse>.Conflict("Complete step 1 before saving step 2.");
        }

        var allRegionIds = request.TargetRegionIds
            .Concat(request.ExcludeRegionIds)
            .Distinct()
            .ToList();

        var activeRegions = await _db.Regions.AsNoTracking()
            .Where(r => allRegionIds.Contains(r.Id) && r.IsActive)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (activeRegions.Count != allRegionIds.Count)
        {
            return Result<CampaignResponse>.ValidationFailure(
            [
                new ValidationError(
                    nameof(request.TargetRegionIds),
                    "One or more region ids are invalid or inactive.")
            ]);
        }

        var existing = await _db.CampaignRegions
            .Where(r => r.CampaignId == campaign.Id)
            .ToListAsync(cancellationToken);
        _db.CampaignRegions.RemoveRange(existing);

        foreach (var regionId in request.TargetRegionIds.Distinct())
        {
            _db.CampaignRegions.Add(new CampaignRegion
            {
                CampaignId = campaign.Id,
                RegionId = regionId,
                Mode = CampaignRegionMode.Target
            });
        }

        foreach (var regionId in request.ExcludeRegionIds.Distinct())
        {
            _db.CampaignRegions.Add(new CampaignRegion
            {
                CampaignId = campaign.Id,
                RegionId = regionId,
                Mode = CampaignRegionMode.Exclude
            });
        }

        campaign.Languages = NormalizeLanguages(request.Languages);
        campaign.AgeRanges = request.AgeRanges.Select(a => a.Trim()).Distinct(StringComparer.Ordinal).ToList();
        campaign.Gender = request.Gender;
        campaign.Interests = request.Interests.Select(i => i.Trim()).Where(i => i.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        campaign.CurrentStep = Math.Max(campaign.CurrentStep, 2);
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignUpdated,
            $"updated \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Step 2 saved successfully.");
    }

    public async Task<Result<CampaignResponse>> SaveStep3Async(
        int organizationId,
        int actorUserId,
        int campaignId,
        SaveCampaignStep3Request request,
        CancellationToken cancellationToken = default)
    {
        var campaignResult = await GetDraftCampaignAsync(organizationId, campaignId, cancellationToken);
        if (campaignResult.IsFailure)
        {
            return MapFailure<CampaignResponse>(campaignResult);
        }

        var campaign = campaignResult.Value;
        if (campaign.CurrentStep < 2)
        {
            return Result<CampaignResponse>.Conflict("Complete step 2 before saving step 3.");
        }

        campaign.DailyBudget = request.DailyBudget;
        campaign.ScheduleType = request.ScheduleType;
        campaign.EndDate = request.ScheduleType == CampaignScheduleType.EndDate ? request.EndDate : null;
        campaign.Headline = request.Headline.Trim();
        campaign.Description = request.Description.Trim();
        campaign.CallToAction = request.CallToAction;
        campaign.CreativeImageUrl = string.IsNullOrWhiteSpace(request.CreativeImageUrl)
            ? null
            : request.CreativeImageUrl.Trim();
        campaign.CurrentStep = Math.Max(campaign.CurrentStep, 3);
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignUpdated,
            $"updated \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Step 3 saved successfully.");
    }

    public async Task<Result<CampaignSummaryResponse>> GetSummaryAsync(
        int organizationId,
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _db.Campaigns.AsNoTracking()
            .Include(c => c.CampaignType)
            .Include(c => c.Regions)
                .ThenInclude(r => r.Region)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignSummaryResponse>.NotFound("Campaign not found.");
        }

        var targets = campaign.Regions
            .Where(r => r.Mode == CampaignRegionMode.Target)
            .OrderBy(r => r.Region.SortOrder)
            .ToList();

        return Result<CampaignSummaryResponse>.Success(new CampaignSummaryResponse
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Status = campaign.Status,
            CurrentStep = campaign.CurrentStep,
            CampaignType = new CampaignTypeSummaryResponse
            {
                Id = campaign.CampaignType.Id,
                Code = campaign.CampaignType.Code,
                NameEn = campaign.CampaignType.NameEn,
                NameAm = campaign.CampaignType.NameAm
            },
            TargetLocationNamesEn = targets.Select(t => t.Region.NameEn).ToList(),
            TargetLocationNamesAm = targets.Select(t => t.Region.NameAm).ToList(),
            DailyBudget = campaign.DailyBudget,
            EstimatedMonthlySpend = campaign.DailyBudget.HasValue
                ? Math.Round(campaign.DailyBudget.Value * CampaignFunding.DaysPerMonthEstimate, 2)
                : null,
            ScheduleType = campaign.ScheduleType,
            EndDate = campaign.EndDate,
            Headline = campaign.Headline,
            Description = campaign.Description,
            CallToAction = campaign.CallToAction,
            CreativeImageUrl = campaign.CreativeImageUrl,
            InitialDepositRequired = CampaignFunding.RequiredBalance(campaign, DateOnly.FromDateTime(DateTime.UtcNow))
        }, "Campaign summary retrieved successfully.");
    }

    public async Task<Result<CampaignResponse>> LaunchAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.Regions)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignResponse>.NotFound("Campaign not found.");
        }

        if (campaign.Status != CampaignStatus.Draft)
        {
            return Result<CampaignResponse>.Conflict("Only draft campaigns can be launched.");
        }

        var validationErrors = ValidateReadyToLaunch(campaign);
        if (validationErrors.Count > 0)
        {
            return Result<CampaignResponse>.ValidationFailure(validationErrors);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var requiredBalance = CampaignFunding.RequiredBalance(campaign, today);
        var availableBalance = await _db.OrganizationWallets.AsNoTracking()
            .Where(w => w.OrganizationId == organizationId)
            .Select(w => (decimal?)w.AvailableBalance)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        if (availableBalance < requiredBalance)
        {
            return Result<CampaignResponse>.Conflict(
                $"Available balance is {availableBalance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} ETB. This campaign requires {requiredBalance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} ETB.");
        }

        campaign.Status = CampaignStatus.Live;
        campaign.CurrentStep = 3;
        campaign.LaunchedAtUtc = DateTimeOffset.UtcNow;
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignLaunched,
            $"launched \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Campaign launched successfully.");
    }

    public async Task<Result<CampaignResponse>> PauseAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignResponse>.NotFound("Campaign not found.");
        }

        if (campaign.Status != CampaignStatus.Live)
        {
            return Result<CampaignResponse>.Conflict("Only live campaigns can be paused.");
        }

        campaign.Status = CampaignStatus.Paused;
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignPaused,
            $"paused \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Campaign paused successfully.");
    }

    public async Task<Result<CampaignResponse>> ResumeAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignResponse>.NotFound("Campaign not found.");
        }

        if (campaign.Status != CampaignStatus.Paused)
        {
            return Result<CampaignResponse>.Conflict("Only paused campaigns can be resumed.");
        }

        campaign.Status = CampaignStatus.Live;
        campaign.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await WriteCampaignAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.CampaignResumed,
            $"resumed \"{campaign.Name}\" campaign.",
            campaign,
            cancellationToken);

        return Result<CampaignResponse>.Success(
            await MapResponseAsync(campaign.Id, organizationId, null, null, cancellationToken),
            "Campaign resumed successfully.");
    }

    private async Task WriteCampaignAuditAsync(
        int organizationId,
        int actorUserId,
        string actionCode,
        string actionSummary,
        Campaign campaign,
        CancellationToken cancellationToken)
    {
        var actor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
        await _auditLogService.WriteAsync(new AuditLogWriteRequest
        {
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            ActorDisplayName = AuditActorNames.Format(actor),
            ActionCode = actionCode,
            ActionSummary = actionSummary,
            HighlightedTarget = campaign.Name,
            Category = AuditLogCategory.Campaign,
            Status = AuditLogStatus.Success,
            ResourceType = "Campaign",
            ResourceId = campaign.Id.ToString(),
            LinkPath = "/dashboard/campaigns",
            LinkText = "View Campaign"
        }, cancellationToken);
    }

    private static Result<TOut> MapFailure<TOut>(Result failed)
    {
        if (failed.ValidationErrors.Count > 0)
        {
            return Result<TOut>.ValidationFailure(failed.ValidationErrors);
        }

        return Result<TOut>.Failure(
            failed.Error,
            failed.StatusCode ?? StatusCodes.Status400BadRequest);
    }

    private async Task<Result<Campaign>> GetDraftCampaignAsync(
        int organizationId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<Campaign>.NotFound("Campaign not found.");
        }

        if (campaign.Status != CampaignStatus.Draft)
        {
            return Result<Campaign>.Conflict("Only draft campaigns can be edited.");
        }

        return Result<Campaign>.Success(campaign);
    }

    private async Task<Result<CampaignType>> GetActiveCampaignTypeAsync(
        int campaignTypeId,
        CancellationToken cancellationToken)
    {
        var campaignType = await _db.CampaignTypes
            .FirstOrDefaultAsync(t => t.Id == campaignTypeId && t.IsActive, cancellationToken);

        if (campaignType is null)
        {
            return Result<CampaignType>.ValidationFailure(
            [
                new ValidationError(
                    nameof(SaveCampaignStep1Request.CampaignTypeId),
                    "Campaign type is invalid or inactive.")
            ]);
        }

        return Result<CampaignType>.Success(campaignType);
    }

    private static List<string> NormalizeLanguages(IEnumerable<string> languages)
        => languages
            .Select(l => l.Trim())
            .Where(l => SaveCampaignStep2RequestValidator.AllowedLanguages.Contains(l))
            .Select(l => string.Equals(l, "Am", StringComparison.OrdinalIgnoreCase) ? "Am" : "En")
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static List<ValidationError> ValidateReadyToLaunch(Campaign campaign)
    {
        var errors = new List<ValidationError>();

        if (campaign.CampaignTypeId <= 0)
        {
            errors.Add(new ValidationError(nameof(campaign.CampaignTypeId), "Campaign type is required."));
        }

        if (!campaign.Regions.Any(r => r.Mode == CampaignRegionMode.Target))
        {
            errors.Add(new ValidationError("TargetRegionIds", "At least one target region is required."));
        }

        if (campaign.Languages.Count == 0)
        {
            errors.Add(new ValidationError(nameof(campaign.Languages), "At least one language is required."));
        }

        if (campaign.AgeRanges.Count == 0)
        {
            errors.Add(new ValidationError(nameof(campaign.AgeRanges), "At least one age range is required."));
        }

        if (!campaign.Gender.HasValue)
        {
            errors.Add(new ValidationError(nameof(campaign.Gender), "Gender is required."));
        }

        if (campaign.Interests.Count == 0)
        {
            errors.Add(new ValidationError(nameof(campaign.Interests), "At least one interest is required."));
        }

        if (!campaign.DailyBudget.HasValue || campaign.DailyBudget <= 0)
        {
            errors.Add(new ValidationError(nameof(campaign.DailyBudget), "Daily budget is required."));
        }

        if (!campaign.ScheduleType.HasValue)
        {
            errors.Add(new ValidationError(nameof(campaign.ScheduleType), "Schedule type is required."));
        }
        else if (campaign.ScheduleType == CampaignScheduleType.EndDate && !campaign.EndDate.HasValue)
        {
            errors.Add(new ValidationError(nameof(campaign.EndDate), "End date is required for EndDate schedule."));
        }
        else if (campaign.ScheduleType == CampaignScheduleType.EndDate
                 && campaign.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            errors.Add(new ValidationError(nameof(campaign.EndDate), "End date must be today or a future date."));
        }

        if (string.IsNullOrWhiteSpace(campaign.Headline))
        {
            errors.Add(new ValidationError(nameof(campaign.Headline), "Headline is required."));
        }

        if (string.IsNullOrWhiteSpace(campaign.Description))
        {
            errors.Add(new ValidationError(nameof(campaign.Description), "Description is required."));
        }

        if (!campaign.CallToAction.HasValue)
        {
            errors.Add(new ValidationError(nameof(campaign.CallToAction), "Call to action is required."));
        }

        if (string.IsNullOrWhiteSpace(campaign.CreativeImageUrl))
        {
            errors.Add(new ValidationError(nameof(campaign.CreativeImageUrl), "Creative image URL is required."));
        }

        return errors;
    }

    private async Task<CampaignResponse> MapResponseAsync(
        int campaignId,
        int organizationId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var campaign = await _db.Campaigns.AsNoTracking()
            .Include(c => c.CampaignType)
            .Include(c => c.Regions)
                .ThenInclude(r => r.Region)
            .FirstAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        decimal? spend = null;
        long? clicks = null;
        decimal? ctr = null;

        if (from.HasValue && to.HasValue)
        {
            var metrics = await _db.CampaignDailyMetrics.AsNoTracking()
                .Where(m => m.CampaignId == campaignId && m.Date >= from.Value && m.Date <= to.Value)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Spend = g.Sum(x => x.Spend),
                    Clicks = g.Sum(x => x.Clicks),
                    Impressions = g.Sum(x => x.Impressions)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (metrics is not null)
            {
                spend = metrics.Spend;
                clicks = metrics.Clicks;
                ctr = metrics.Impressions == 0
                    ? 0
                    : Math.Round((decimal)metrics.Clicks * 100m / metrics.Impressions, 2);
            }
            else
            {
                spend = 0;
                clicks = 0;
                ctr = 0;
            }
        }

        return new CampaignResponse
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Status = campaign.Status,
            CurrentStep = campaign.CurrentStep,
            CampaignType = new CampaignTypeSummaryResponse
            {
                Id = campaign.CampaignType.Id,
                Code = campaign.CampaignType.Code,
                NameEn = campaign.CampaignType.NameEn,
                NameAm = campaign.CampaignType.NameAm
            },
            Regions = campaign.Regions
                .OrderBy(r => r.Region.SortOrder)
                .Select(r => new CampaignRegionItemResponse
                {
                    RegionId = r.RegionId,
                    Code = r.Region.Code,
                    NameEn = r.Region.NameEn,
                    NameAm = r.Region.NameAm,
                    Mode = r.Mode
                })
                .ToList(),
            Languages = campaign.Languages,
            AgeRanges = campaign.AgeRanges,
            Gender = campaign.Gender,
            Interests = campaign.Interests,
            DailyBudget = campaign.DailyBudget,
            ScheduleType = campaign.ScheduleType,
            EndDate = campaign.EndDate,
            Headline = campaign.Headline,
            Description = campaign.Description,
            CallToAction = campaign.CallToAction,
            CreativeImageUrl = campaign.CreativeImageUrl,
            Spend = spend,
            Clicks = clicks,
            Ctr = ctr,
            CreatedAtUtc = campaign.CreatedAtUtc,
            UpdatedAtUtc = campaign.UpdatedAtUtc,
            LaunchedAtUtc = campaign.LaunchedAtUtc
        };
    }
}
