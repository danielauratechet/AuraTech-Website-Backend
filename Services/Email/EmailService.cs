using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace AuratechBackend.Services.Email;

public sealed class EmailService : IEmailSender
{
    private const string HttpClientName = "Bird";

    private readonly EmailSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
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
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("Bird API key is not configured (EmailSettings:ApiKey).");
            }

            var baseUrl = (_settings.ApiBaseUrl ?? "https://eu1.platform.bird.com/v1").TrimEnd('/');
            var fromEmail = _settings.FromEmail ?? "noreply@auratech.local";
            var fromName = _settings.FromName ?? "AuraTech";

            var payload = new BirdEmailRequest
            {
                From = new BirdFrom
                {
                    Email = fromEmail,
                    Name = fromName
                },
                To = [toEmail],
                Subject = subject ?? string.Empty,
                Html = htmlBody ?? string.Empty,
                Category = "transactional"
            };

            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/email/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            request.Content = JsonContent.Create(payload);

            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Bird rejected email to {To}. Status={StatusCode}, Body={Body}",
                    toEmail,
                    (int)response.StatusCode,
                    responseBody);
                throw new InvalidOperationException(
                    $"Bird email send failed with status {(int)response.StatusCode}.");
            }

            _logger.LogInformation(
                "Bird accepted message for {To}. Status={StatusCode}",
                toEmail,
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
            throw;
        }
    }

    private sealed class BirdEmailRequest
    {
        [JsonPropertyName("from")]
        public required BirdFrom From { get; init; }

        [JsonPropertyName("to")]
        public required string[] To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("html")]
        public required string Html { get; init; }

        [JsonPropertyName("category")]
        public required string Category { get; init; }
    }

    private sealed class BirdFrom
    {
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }
}
