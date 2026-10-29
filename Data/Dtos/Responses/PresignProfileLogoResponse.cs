namespace AuratechBackend.Data.Dtos.Responses;

public sealed class PresignProfileLogoResponse
{
    public string UploadUrl { get; init; } = string.Empty;

    public string PublicUrl { get; init; } = string.Empty;

    public string ObjectKey { get; init; } = string.Empty;
}
