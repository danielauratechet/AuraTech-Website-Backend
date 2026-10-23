using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Campaigns;

public sealed class CampaignAnalyticsService : ICampaignAnalyticsService
{
    private readonly AppDbContext _db;

    public CampaignAnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CampaignAnalyticsOverviewResponse>> GetOverviewAsync(
        int organizationId,
        CampaignAnalyticsOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = query.To ?? today;
        var from = query.From ?? to.AddDays(-29);

        if (from > to)
        {
            return Result<CampaignAnalyticsOverviewResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.From), "From must be on or before To.")
            ]);
        }

        if (to.DayNumber - from.DayNumber > 366)
        {
            return Result<CampaignAnalyticsOverviewResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.To), "Date range cannot exceed 366 days.")
            ]);
        }

        var periodDays = to.DayNumber - from.DayNumber + 1;
        var priorTo = from.AddDays(-1);
        var priorFrom = priorTo.AddDays(-(periodDays - 1));

        var campaignIds = await _db.Campaigns.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var currentTotals = await SumDailyMetricsAsync(campaignIds, from, to, cancellationToken);
        var priorTotals = await SumDailyMetricsAsync(campaignIds, priorFrom, priorTo, cancellationToken);

        var trendRows = await _db.CampaignDailyMetrics.AsNoTracking()
            .Where(m => campaignIds.Contains(m.CampaignId) && m.Date >= from && m.Date <= to)
            .GroupBy(m => m.Date)
            .Select(g => new
            {
                Date = g.Key,
                Clicks = g.Sum(x => x.Clicks),
                Impressions = g.Sum(x => x.Impressions)
            })
            .ToListAsync(cancellationToken);

        var trendByDate = trendRows.ToDictionary(x => x.Date);
        var trend = new List<AnalyticsTrendPointResponse>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            trendByDate.TryGetValue(d, out var point);
            trend.Add(new AnalyticsTrendPointResponse
            {
                Date = d,
                Clicks = point?.Clicks ?? 0,
                Impressions = point?.Impressions ?? 0
            });
        }

        var deviceTotals = await _db.CampaignDeviceDailyMetrics.AsNoTracking()
            .Where(m => campaignIds.Contains(m.CampaignId) && m.Date >= from && m.Date <= to)
            .GroupBy(m => m.Device)
            .Select(g => new { Device = g.Key, Impressions = g.Sum(x => x.Impressions) })
            .ToListAsync(cancellationToken);

        var mobile = deviceTotals.FirstOrDefault(x => x.Device == CampaignDevice.Mobile)?.Impressions ?? 0;
        var desktop = deviceTotals.FirstOrDefault(x => x.Device == CampaignDevice.Desktop)?.Impressions ?? 0;
        var tablet = deviceTotals.FirstOrDefault(x => x.Device == CampaignDevice.Tablet)?.Impressions ?? 0;
        var deviceSum = mobile + desktop + tablet;

        var deviceBreakdown = deviceSum == 0
            ? new AnalyticsDeviceBreakdownResponse()
            : new AnalyticsDeviceBreakdownResponse
            {
                MobilePercent = Math.Round(mobile * 100m / deviceSum, 2),
                DesktopPercent = Math.Round(desktop * 100m / deviceSum, 2),
                TabletPercent = Math.Round(tablet * 100m / deviceSum, 2)
            };

        var regionTotals = await _db.CampaignRegionDailyMetrics.AsNoTracking()
            .Where(m => campaignIds.Contains(m.CampaignId) && m.Date >= from && m.Date <= to)
            .GroupBy(m => m.RegionId)
            .Select(g => new { RegionId = g.Key, Impressions = g.Sum(x => x.Impressions) })
            .OrderByDescending(x => x.Impressions)
            .ToListAsync(cancellationToken);

        var regionIds = regionTotals.Select(x => x.RegionId).ToList();
        var regions = await _db.Regions.AsNoTracking()
            .Where(r => regionIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var geoTotal = regionTotals.Sum(x => x.Impressions);
        var geographicReach = new List<AnalyticsGeoReachItemResponse>();
        if (geoTotal > 0)
        {
            var top = regionTotals.Take(3).ToList();
            var otherImpressions = regionTotals.Skip(3).Sum(x => x.Impressions);

            foreach (var item in top)
            {
                regions.TryGetValue(item.RegionId, out var region);
                geographicReach.Add(new AnalyticsGeoReachItemResponse
                {
                    RegionId = item.RegionId,
                    Code = region?.Code ?? "UNK",
                    NameEn = region?.NameEn ?? "Unknown",
                    NameAm = region?.NameAm ?? "Unknown",
                    Percent = Math.Round(item.Impressions * 100m / geoTotal, 2)
                });
            }

            if (otherImpressions > 0 || regionTotals.Count > 3)
            {
                geographicReach.Add(new AnalyticsGeoReachItemResponse
                {
                    RegionId = null,
                    Code = "OTHER",
                    NameEn = "Other Regions",
                    NameAm = "ሌሎች ክልሎች",
                    Percent = Math.Round(otherImpressions * 100m / geoTotal, 2)
                });
            }
        }

        var campaignLimit = query.CampaignLimit <= 0 ? 10 : Math.Min(query.CampaignLimit, 100);
        var performanceRows = await _db.CampaignDailyMetrics.AsNoTracking()
            .Where(m => campaignIds.Contains(m.CampaignId) && m.Date >= from && m.Date <= to)
            .GroupBy(m => m.CampaignId)
            .Select(g => new
            {
                CampaignId = g.Key,
                Spend = g.Sum(x => x.Spend),
                Clicks = g.Sum(x => x.Clicks),
                Impressions = g.Sum(x => x.Impressions)
            })
            .OrderByDescending(x => x.Spend)
            .Take(campaignLimit)
            .ToListAsync(cancellationToken);

        var perfCampaignIds = performanceRows.Select(x => x.CampaignId).ToList();
        var campaigns = await _db.Campaigns.AsNoTracking()
            .Where(c => perfCampaignIds.Contains(c.Id)
                        && c.OrganizationId == organizationId
                        && (c.Status == CampaignStatus.Live || c.Status == CampaignStatus.Paused))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        // Prefer campaigns that appear in metrics; if none, fall back to latest live/paused with zero metrics.
        var campaignPerformance = new List<AnalyticsCampaignPerformanceItemResponse>();
        foreach (var row in performanceRows)
        {
            if (!campaigns.TryGetValue(row.CampaignId, out var campaign))
            {
                continue;
            }

            campaignPerformance.Add(new AnalyticsCampaignPerformanceItemResponse
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Status = campaign.Status,
                Spend = row.Spend,
                Clicks = row.Clicks,
                Ctr = row.Impressions == 0
                    ? 0
                    : Math.Round((decimal)row.Clicks * 100m / row.Impressions, 2)
            });
        }

        if (campaignPerformance.Count == 0)
        {
            var fallback = await _db.Campaigns.AsNoTracking()
                .Where(c => c.OrganizationId == organizationId
                            && (c.Status == CampaignStatus.Live || c.Status == CampaignStatus.Paused))
                .OrderByDescending(c => c.LaunchedAtUtc ?? c.CreatedAtUtc)
                .Take(campaignLimit)
                .Select(c => new AnalyticsCampaignPerformanceItemResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status,
                    Spend = 0,
                    Clicks = 0,
                    Ctr = 0
                })
                .ToListAsync(cancellationToken);

            campaignPerformance = fallback;
        }

        var currentCtr = currentTotals.Impressions == 0
            ? 0
            : Math.Round((decimal)currentTotals.Clicks * 100m / currentTotals.Impressions, 2);
        var priorCtr = priorTotals.Impressions == 0
            ? 0
            : Math.Round((decimal)priorTotals.Clicks * 100m / priorTotals.Impressions, 2);

        var response = new CampaignAnalyticsOverviewResponse
        {
            From = from,
            To = to,
            TotalSpend = BuildCard(currentTotals.Spend, priorTotals.Spend),
            Impressions = BuildCard(currentTotals.Impressions, priorTotals.Impressions),
            TotalClicks = BuildCard(currentTotals.Clicks, priorTotals.Clicks),
            AvgCtr = BuildCard(currentCtr, priorCtr),
            PerformanceTrend = trend,
            DeviceBreakdown = deviceBreakdown,
            GeographicReach = geographicReach,
            CampaignPerformance = campaignPerformance
        };

        return Result<CampaignAnalyticsOverviewResponse>.Success(response, "Analytics overview retrieved successfully.");
    }

    private async Task<(decimal Spend, long Impressions, long Clicks)> SumDailyMetricsAsync(
        IReadOnlyCollection<int> campaignIds,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        if (campaignIds.Count == 0)
        {
            return (0, 0, 0);
        }

        var totals = await _db.CampaignDailyMetrics.AsNoTracking()
            .Where(m => campaignIds.Contains(m.CampaignId) && m.Date >= from && m.Date <= to)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Spend = g.Sum(x => x.Spend),
                Impressions = g.Sum(x => x.Impressions),
                Clicks = g.Sum(x => x.Clicks)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null
            ? (0, 0, 0)
            : (totals.Spend, totals.Impressions, totals.Clicks);
    }

    private static AnalyticsMetricCardResponse BuildCard(decimal current, decimal prior)
    {
        decimal? change = null;
        if (prior != 0)
        {
            change = Math.Round((current - prior) * 100m / prior, 2);
        }
        else if (current != 0)
        {
            change = 100m;
        }

        return new AnalyticsMetricCardResponse
        {
            Value = current,
            ChangePercent = change
        };
    }

    private static AnalyticsMetricCardResponse BuildCard(long current, long prior)
        => BuildCard((decimal)current, (decimal)prior);
}
