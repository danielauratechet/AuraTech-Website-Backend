using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignSummaryResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CampaignStatus Status { get; init; }

    public int CurrentStep { get; init; }

    public CampaignTypeSummaryResponse CampaignType { get; init; } = null!;

    public IReadOnlyList<string> TargetLocationNamesEn { get; init; } = [];

    public IReadOnlyList<string> TargetLocationNamesAm { get; init; } = [];

    public decimal? DailyBudget { get; init; }

    public decimal? EstimatedMonthlySpend { get; init; }

    public CampaignScheduleType? ScheduleType { get; init; }

    public DateOnly? EndDate { get; init; }

    public string? Headline { get; init; }

    public string? Description { get; init; }

    public CampaignCallToAction? CallToAction { get; init; }

    public string? CreativeImageUrl { get; init; }

    public decimal InitialDepositRequired { get; init; } = 1500m;
}
