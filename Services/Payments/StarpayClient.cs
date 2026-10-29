using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace AuratechBackend.Services.Payments;

public sealed class StarpayClient : IStarpayClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _http;
    private readonly StarpaySettings _settings;
    private readonly ILogger<StarpayClient> _logger;

    public StarpayClient(HttpClient http, IOptions<StarpaySettings> settings, ILogger<StarpayClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<StarpayCreateOrderResult> CreateOrderAsync(
        StarpayCreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured(out var configurationError))
        {
            return StarpayCreateOrderResult.Failure(configurationError);
        }

        var payload = new StarpayOrderRequest
        {
            Amount = command.Amount,
            Currency = "ETB",
            Description = command.Description,
            CustomerName = command.CustomerName,
            CustomerPhoneNumber = command.CustomerPhoneNumber,
            CallbackUrl = command.CallbackUrl,
            RedirectUrl = command.RedirectUrl
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("trdp/order"))
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            request.Headers.TryAddWithoutValidation("x-api-secret", _settings.SecretKey);

            using var response = await _http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("StarPay create order failed with status {StatusCode}", (int)response.StatusCode);
                return StarpayCreateOrderResult.Failure(ReadErrorMessage(body) ?? "StarPay could not create the payment.");
            }

            using var document = JsonDocument.Parse(body);
            var data = ReadData(document.RootElement);
            var billRefNo = ReadString(data, "bill_ref_no", "billRefNo", "order_id", "orderId");
            var paymentUrl = ReadString(data, "payment_url", "paymentUrl");
            if (string.IsNullOrWhiteSpace(billRefNo) || string.IsNullOrWhiteSpace(paymentUrl))
            {
                return StarpayCreateOrderResult.Failure("StarPay did not return a payment URL.");
            }

            return StarpayCreateOrderResult.Success(billRefNo, paymentUrl);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "StarPay create order request failed");
            return StarpayCreateOrderResult.Failure("StarPay could not create the payment.");
        }
    }

    public async Task<StarpayVerifyResult> VerifyAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured(out var configurationError))
        {
            return new StarpayVerifyResult { CallFailed = true, ErrorMessage = configurationError };
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("trdp/verify"))
            {
                Content = JsonContent.Create(new StarpayVerifyRequest { OrderId = orderId }, options: JsonOptions)
            };
            request.Headers.TryAddWithoutValidation("x-api-secret", _settings.SecretKey);

            using var response = await _http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("StarPay verify failed with status {StatusCode} for order {OrderId}", (int)response.StatusCode, orderId);
                return new StarpayVerifyResult
                {
                    CallFailed = true,
                    ErrorMessage = ReadErrorMessage(body) ?? "StarPay could not verify the payment."
                };
            }

            using var document = JsonDocument.Parse(body);
            var data = ReadData(document.RootElement);
            return new StarpayVerifyResult
            {
                Status = ReadString(data, "status"),
                Amount = ReadDecimal(data, "amount")
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "StarPay verify request failed for order {OrderId}", orderId);
            return new StarpayVerifyResult
            {
                CallFailed = true,
                ErrorMessage = "StarPay could not verify the payment."
            };
        }
    }

    private bool IsConfigured(out string error)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            error = "Wallet top-up is not configured.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private string BuildUrl(string relativePath)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/{relativePath}";
    }

    private static JsonElement ReadData(JsonElement root)
        => root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
            ? data
            : root;

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static decimal? ReadDecimal(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? ReadErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var message = ReadString(root, "message");
            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                message = ReadString(error, "message") ?? message;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            return message.Length <= 300 ? message : message[..300];
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class StarpayOrderRequest
    {
        public decimal Amount { get; init; }

        public string Currency { get; init; } = "ETB";

        public string Description { get; init; } = string.Empty;

        public string CustomerName { get; init; } = string.Empty;

        public string CustomerPhoneNumber { get; init; } = string.Empty;

        [JsonPropertyName("callbackURL")]
        public string CallbackUrl { get; init; } = string.Empty;

        public string? RedirectUrl { get; init; }
    }

    private sealed class StarpayVerifyRequest
    {
        public string OrderId { get; init; } = string.Empty;
    }
}
