namespace AuratechBackend.Data.Entities;

public class CampaignRegionDailyMetric
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int RegionId { get; set; }

    public Region Region { get; set; } = null!;

    public long Impressions { get; set; }
}
