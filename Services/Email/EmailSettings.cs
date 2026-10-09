namespace AuratechBackend.Services.Email;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string ApiBaseUrl { get; set; } = "https://eu1.platform.bird.com/v1";

    public string? ApiKey { get; set; }

    public string? FromEmail { get; set; }

    public string? FromName { get; set; }
}
