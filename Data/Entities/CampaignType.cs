namespace AuratechBackend.Data.Entities;

public class CampaignType
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string NameAm { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public string DescriptionAm { get; set; } = string.Empty;

    public string? IconKey { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
