using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CampaignStatus Status { get; init; }

    public int CurrentStep { get; init; }

    public CampaignTypeSummaryResponse CampaignType { get; init; } = null!;

    public IReadOnlyList<CampaignRegionItemResponse> Regions { get; init; } = [];

    public IReadOnlyList<string> Languages { get; init; } = [];

    public IReadOnlyList<string> AgeRanges { get; init; } = [];

    public CampaignGender? Gender { get; init; }

    public IReadOnlyList<string> Interests { get; init; } = [];

    public decimal? DailyBudget { get; init; }

    public CampaignScheduleType? ScheduleType { get; init; }

    public DateOnly? EndDate { get; init; }

    public string? Headline { get; init; }

    public string? Description { get; init; }

    public CampaignCallToAction? CallToAction { get; init; }

    public string? CreativeImageUrl { get; init; }

    public decimal? Spend { get; init; }

    public long? Clicks { get; init; }

    public decimal? Ctr { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? UpdatedAtUtc { get; init; }

    public DateTimeOffset? LaunchedAtUtc { get; init; }
}
