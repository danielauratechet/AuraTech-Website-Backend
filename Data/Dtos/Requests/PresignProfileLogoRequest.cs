namespace AuratechBackend.Data.Dtos.Requests;

public sealed class PresignProfileLogoRequest
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;
}
