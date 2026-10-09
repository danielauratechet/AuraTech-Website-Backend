namespace AuratechBackend.Data.Dtos.Requests;

public sealed class PresignKycUploadRequest
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    /// <summary>Either "tin_certificate" or "government_id".</summary>
    public string Kind { get; init; } = string.Empty;
}
