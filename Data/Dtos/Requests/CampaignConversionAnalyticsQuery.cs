namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CampaignConversionAnalyticsQuery
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}
