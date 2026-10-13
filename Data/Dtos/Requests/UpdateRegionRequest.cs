namespace AuratechBackend.Data.Dtos.Requests;

public sealed class UpdateRegionRequest
{
    public string NameEn { get; init; } = string.Empty;

    public string NameAm { get; init; } = string.Empty;

    public long EstimatedUsers { get; init; }

    public int SortOrder { get; init; }
}
