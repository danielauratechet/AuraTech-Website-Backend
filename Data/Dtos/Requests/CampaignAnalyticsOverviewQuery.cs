namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CampaignAnalyticsOverviewQuery
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public int CampaignLimit { get; init; } = 10;
}
