using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuratechBackend.Services.Email;

public sealed class EmailService : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> options,
        IHostEnvironment environment,
        ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Skipping email send because recipient is empty.");
            return;
        }

        try
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("=== EMAIL (Suppressed - {Environment}) ===", _environment.EnvironmentName);
                _logger.LogInformation("To: {To}", toEmail);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body Length: {Length} characters", htmlBody?.Length ?? 0);

                try
                {
                    var match = Regex.Match(htmlBody ?? string.Empty, @"https?://[^""'\s<>]+");
                    if (match.Success)
                    {
                        _logger.LogInformation("Detected Link: {Link}", match.Value);
                    }
                }
                catch
                {
                    // ignore regex issues
                }

                _logger.LogInformation("Email sending skipped (Development/Local environment).\n================================");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _settings.FromName ?? "AuraTech",
                _settings.FromEmail ?? "noreply@auratech.local"));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = subject ?? string.Empty;
            message.Body = new BodyBuilder { HtmlBody = htmlBody ?? string.Empty }.ToMessageBody();

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;

            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                throw new InvalidOperationException("EmailSettings:SmtpHost is not configured.");
            }

            var smtpPort = int.TryParse(_settings.SmtpPort, out var port) ? port : 587;
            var enableSsl = !bool.TryParse(_settings.EnableSsl, out var ssl) || ssl;

            await client.ConnectAsync(
                _settings.SmtpHost,
                smtpPort,
                enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.SmtpUser) && !string.IsNullOrWhiteSpace(_settings.SmtpPassword))
            {
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
            throw;
        }
    }
}
