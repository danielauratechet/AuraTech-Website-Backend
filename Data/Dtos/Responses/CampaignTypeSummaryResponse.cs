namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignTypeSummaryResponse
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;
}
