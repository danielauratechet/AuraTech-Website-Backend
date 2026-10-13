namespace AuratechBackend.Data.Dtos.Requests;

public sealed class UpdateCampaignTypeRequest
{
    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public string DescriptionAm { get; init; } = string.Empty;

    public string? IconKey { get; init; }

    public int SortOrder { get; init; }
}
