using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignConversionAnalyticsResponse
{
    public int CampaignId { get; init; }

    public string CampaignName { get; init; } = string.Empty;

    public ConversionType ConversionType { get; init; }

    public string GoalName { get; init; } = string.Empty;

    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public IReadOnlyList<ConversionInteractionItemResponse> Interactions { get; init; } = [];

    public long TotalConversions { get; init; }

    public decimal ConversionValueEtb { get; init; }

    public string TrackingStatus { get; init; } = string.Empty;
}

public sealed class ConversionInteractionItemResponse
{
    public string Code { get; init; } = string.Empty;

    public string LabelEn { get; init; } = string.Empty;

    public string LabelAm { get; init; } = string.Empty;

    public long Count { get; init; }
}
