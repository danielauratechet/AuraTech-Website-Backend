namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CampaignTypeResponse
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public string DescriptionAm { get; init; } = string.Empty;

    public string? IconKey { get; init; }

    public int SortOrder { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? UpdatedAtUtc { get; init; }
}
