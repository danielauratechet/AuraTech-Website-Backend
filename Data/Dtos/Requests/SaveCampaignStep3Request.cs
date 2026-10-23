using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class SaveCampaignStep3Request
{
    public decimal DailyBudget { get; set; }

    public CampaignScheduleType ScheduleType { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Headline { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public CampaignCallToAction CallToAction { get; set; }

    public string? CreativeImageUrl { get; set; }
}
