namespace AuratechBackend.Data.Entities;

public class CampaignDailyMetric
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public DateOnly Date { get; set; }

    public long Impressions { get; set; }

    public long Clicks { get; set; }

    public decimal Spend { get; set; }
}
