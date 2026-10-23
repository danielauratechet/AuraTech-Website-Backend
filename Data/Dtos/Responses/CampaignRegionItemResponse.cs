using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignRegionItemResponse
{
    public int RegionId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;

    public CampaignRegionMode Mode { get; init; }
}
