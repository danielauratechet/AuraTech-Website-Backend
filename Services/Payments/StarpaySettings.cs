namespace AuratechBackend.Services.Payments;

public sealed class StarpaySettings
{
    public const string SectionName = "StarpaySettings";

    public string BaseUrl { get; set; } = "https://api.starpayethiopia.com";

    public string SecretKey { get; set; } = string.Empty;

    public string WebhookSecretKey { get; set; } = string.Empty;

    public string WebhookUrl { get; set; } = string.Empty;
}
