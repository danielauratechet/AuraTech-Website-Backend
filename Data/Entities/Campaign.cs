using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class Campaign
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int CampaignTypeId { get; set; }

    public CampaignType CampaignType { get; set; } = null!;

    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    public int CurrentStep { get; set; } = 1;

    public List<string> Languages { get; set; } = [];

    public List<string> AgeRanges { get; set; } = [];

    public CampaignGender? Gender { get; set; }

    public List<string> Interests { get; set; } = [];

    public decimal? DailyBudget { get; set; }

    public CampaignScheduleType? ScheduleType { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Headline { get; set; }

    public string? Description { get; set; }

    public CampaignCallToAction? CallToAction { get; set; }

    public string? CreativeImageUrl { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public DateTimeOffset? LaunchedAtUtc { get; set; }

    public ICollection<CampaignRegion> Regions { get; set; } = [];

    public ICollection<CampaignDailyMetric> DailyMetrics { get; set; } = [];

    public ICollection<CampaignDeviceDailyMetric> DeviceDailyMetrics { get; set; } = [];

    public ICollection<CampaignRegionDailyMetric> RegionDailyMetrics { get; set; } = [];
}
