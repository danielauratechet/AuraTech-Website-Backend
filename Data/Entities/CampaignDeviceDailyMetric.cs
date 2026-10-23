using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class CampaignDeviceDailyMetric
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public DateOnly Date { get; set; }

    public CampaignDevice Device { get; set; }

    public long Impressions { get; set; }

    public long Clicks { get; set; }
}
