using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.ConversionTracking;

public sealed class ConversionTrackingService : IConversionTrackingService
{
    private readonly AppDbContext _db;

    public ConversionTrackingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ConversionTrackingResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConversionTrackings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);

        if (entity is null)
        {
            return Result<ConversionTrackingResponse>.NotFound("Conversion tracking has not been configured yet.");
        }

        return Result<ConversionTrackingResponse>.Success(Map(entity), "Conversion tracking retrieved successfully.");
    }

    public async Task<Result<ConversionTrackingResponse>> UpsertAsync(
        int organizationId,
        UpsertConversionTrackingRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConversionTrackings
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);

        if (entity is null)
        {
            entity = new Data.Entities.ConversionTracking
            {
                OrganizationId = organizationId,
                TrackingCode = GenerateTrackingCode(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            _db.ConversionTrackings.Add(entity);
        }
        else
        {
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        entity.ConversionType = request.ConversionType;
        entity.GoalName = request.GoalName.Trim();
        entity.CountMode = request.CountMode;
        entity.ConversionValueEtb = request.ConversionValueEtb;
        entity.ClickThroughWindowDays = request.ClickThroughWindowDays;
        entity.InstallMethod = request.InstallMethod;
        entity.Status = request.Status;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<ConversionTrackingResponse>.Success(Map(entity), "Conversion tracking saved successfully.");
    }

    public async Task<Result<CampaignConversionAnalyticsResponse>> GetCampaignAnalyticsAsync(
        int organizationId,
        int campaignId,
        CampaignConversionAnalyticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _db.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OrganizationId == organizationId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignConversionAnalyticsResponse>.NotFound("Campaign not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = query.To ?? today;
        var from = query.From ?? to.AddDays(-29);

        if (from > to)
        {
            return Result<CampaignConversionAnalyticsResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.From), "From must be on or before To.")
            ]);
        }

        if (to.DayNumber - from.DayNumber > 366)
        {
            return Result<CampaignConversionAnalyticsResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.To), "Date range cannot exceed 366 days.")
            ]);
        }

        var tracking = await _db.ConversionTrackings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);

        var conversionType = tracking?.ConversionType ?? ConversionType.Website;
        var goalName = tracking?.GoalName ?? "Default Website Goal";
        var valueEtb = tracking?.ConversionValueEtb ?? 0m;
        var trackingStatus = tracking is null
            ? "Inactive"
            : tracking.Status == ConversionTrackingStatus.Active ? "Active" : "Inactive";

        var daySpan = Math.Max(1, to.DayNumber - from.DayNumber + 1);
        var seed = HashCode.Combine(campaignId, from.DayNumber, to.DayNumber, (int)conversionType);
        var rng = new Random(seed);

        var interactions = BuildMockInteractions(conversionType, daySpan, rng);
        var totalConversions = interactions.Count > 0
            ? interactions[^1].Count
            : 0L;

        return Result<CampaignConversionAnalyticsResponse>.Success(new CampaignConversionAnalyticsResponse
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            ConversionType = conversionType,
            GoalName = goalName,
            From = from,
            To = to,
            Interactions = interactions,
            TotalConversions = totalConversions,
            ConversionValueEtb = Math.Round(totalConversions * valueEtb, 2),
            TrackingStatus = trackingStatus
        }, "Campaign conversion analytics retrieved successfully.");
    }

    private static ConversionTrackingResponse Map(Data.Entities.ConversionTracking entity)
        => new()
        {
            Id = entity.Id,
            ConversionType = entity.ConversionType,
            GoalName = entity.GoalName,
            CountMode = entity.CountMode,
            ConversionValueEtb = entity.ConversionValueEtb,
            ClickThroughWindowDays = entity.ClickThroughWindowDays,
            InstallMethod = entity.InstallMethod,
            Status = entity.Status,
            TrackingCode = entity.TrackingCode,
            TagSnippet = BuildTagSnippet(entity.OrganizationId, entity.TrackingCode),
            TrackingStatus = entity.Status == ConversionTrackingStatus.Active ? "Active" : "Inactive",
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static string BuildTagSnippet(int organizationId, string trackingCode)
        => $$"""
           <!-- AuraTech Conversion Tag -->
           <script>
           (function(w,d,t,c,o){
             w.AuraTech=w.AuraTech||function(){(w.AuraTech.q=w.AuraTech.q||[]).push(arguments)};
             var s=d.createElement(t);s.async=1;s.src='https://cdn.auratechet.net/ct.js';
             s.setAttribute('data-org',o);s.setAttribute('data-code',c);
             var f=d.getElementsByTagName(t)[0];f.parentNode.insertBefore(s,f);
           })(window,document,'script','{{trackingCode}}','{{organizationId}}');
           </script>
           <!-- End AuraTech Conversion Tag -->
           """;

    private static string GenerateTrackingCode()
        => ("AT-" + Guid.NewGuid().ToString("N")[..16]).ToUpperInvariant();

    private static List<ConversionInteractionItemResponse> BuildMockInteractions(
        ConversionType type,
        int daySpan,
        Random rng)
    {
        long TopOfFunnel(int baseDaily)
            => Math.Max(1, (long)(baseDaily * daySpan * (0.75 + rng.NextDouble() * 0.5)));

        long Next(long previous, double minRate, double maxRate)
            => Math.Max(0, (long)(previous * (minRate + rng.NextDouble() * (maxRate - minRate))));

        if (type == ConversionType.AppInstalls)
        {
            var clicks = TopOfFunnel(120);
            var installs = Next(clicks, 0.22, 0.34);
            var registrations = Next(installs, 0.40, 0.60);
            var inApp = Next(registrations, 0.45, 0.70);
            return
            [
                Item("clicks", "Clicks", "ጠቅታዎች", clicks),
                Item("installs", "App Installs", "የመተግበሪያ ጭነቶች", installs),
                Item("registrations", "Registrations", "ምዝገባዎች", registrations),
                Item("in_app_actions", "In-App Actions", "በመተግበሪያ ውስጥ እርምጃዎች", inApp)
            ];
        }

        if (type == ConversionType.PhoneCalls)
        {
            var clicks = TopOfFunnel(90);
            var calls = Next(clicks, 0.18, 0.28);
            var connected = Next(calls, 0.45, 0.65);
            return
            [
                Item("clicks", "Clicks", "ጠቅታዎች", clicks),
                Item("calls", "Phone Calls", "የስልክ ጥሪዎች", calls),
                Item("connected_calls", "Connected Calls", "የተገናኙ ጥሪዎች", connected)
            ];
        }

        {
            var clicks = TopOfFunnel(150);
            var pageViews = Next(clicks, 0.55, 0.75);
            var signups = Next(pageViews, 0.20, 0.35);
            var purchases = Next(signups, 0.25, 0.45);
            return
            [
                Item("clicks", "Clicks", "ጠቅታዎች", clicks),
                Item("page_views", "Page Views", "የገጽ እይታዎች", pageViews),
                Item("signups", "Sign-ups", "ምዝገባዎች", signups),
                Item("purchases", "Purchases", "ግዢዎች", purchases)
            ];
        }
    }

    private static ConversionInteractionItemResponse Item(string code, string labelEn, string labelAm, long count)
        => new()
        {
            Code = code,
            LabelEn = labelEn,
            LabelAm = labelAm,
            Count = count
        };
}
