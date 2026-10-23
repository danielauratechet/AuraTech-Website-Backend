using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignAnalyticsOverviewResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public AnalyticsMetricCardResponse TotalSpend { get; init; } = null!;

    public AnalyticsMetricCardResponse Impressions { get; init; } = null!;

    public AnalyticsMetricCardResponse TotalClicks { get; init; } = null!;

    public AnalyticsMetricCardResponse AvgCtr { get; init; } = null!;

    public IReadOnlyList<AnalyticsTrendPointResponse> PerformanceTrend { get; init; } = [];

    public AnalyticsDeviceBreakdownResponse DeviceBreakdown { get; init; } = null!;

    public IReadOnlyList<AnalyticsGeoReachItemResponse> GeographicReach { get; init; } = [];

    public IReadOnlyList<AnalyticsCampaignPerformanceItemResponse> CampaignPerformance { get; init; } = [];
}

public sealed class AnalyticsMetricCardResponse
{
    public decimal Value { get; init; }

    public decimal? ChangePercent { get; init; }
}

public sealed class AnalyticsTrendPointResponse
{
    public DateOnly Date { get; init; }

    public long Clicks { get; init; }

    public long Impressions { get; init; }
}

public sealed class AnalyticsDeviceBreakdownResponse
{
    public decimal MobilePercent { get; init; }

    public decimal DesktopPercent { get; init; }

    public decimal TabletPercent { get; init; }
}

public sealed class AnalyticsGeoReachItemResponse
{
    public int? RegionId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;

    public decimal Percent { get; init; }
}

public sealed class AnalyticsCampaignPerformanceItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CampaignStatus Status { get; init; }

    public decimal Spend { get; init; }

    public long Clicks { get; init; }

    public decimal Ctr { get; init; }
}
