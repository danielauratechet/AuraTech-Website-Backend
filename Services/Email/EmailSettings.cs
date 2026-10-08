namespace AuratechBackend.Services.Email;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = "smtp.resend.com";
    public string SmtpPort { get; set; } = "587";
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string EnableSsl { get; set; } = "true";
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}
