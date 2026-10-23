using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class SaveCampaignStep2Request
{
    public List<int> TargetRegionIds { get; set; } = [];

    public List<int> ExcludeRegionIds { get; set; } = [];

    public List<string> Languages { get; set; } = [];

    public List<string> AgeRanges { get; set; } = [];

    public CampaignGender Gender { get; set; } = CampaignGender.All;

    public List<string> Interests { get; set; } = [];
}
