using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class CampaignRegion
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int RegionId { get; set; }

    public Region Region { get; set; } = null!;

    public CampaignRegionMode Mode { get; set; }
}
