using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AuratechBackend.Services.Storage;

public sealed class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;
    private readonly ILogger<SupabaseStorageService> _logger;

    public SupabaseStorageService(
        HttpClient httpClient,
        IOptions<SupabaseOptions> options,
        ILogger<SupabaseStorageService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(string UploadUrl, string PublicUrl)> CreateSignedUploadUrlAsync(string objectKey, string fileName)
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new InvalidOperationException("Supabase Url is not configured");

        if (string.IsNullOrWhiteSpace(_options.ServiceRoleKey))
            throw new InvalidOperationException("Supabase ServiceRoleKey is not configured");

        if (string.IsNullOrWhiteSpace(_options.Bucket))
            throw new InvalidOperationException("Supabase Bucket is not configured");

        var baseUrl = _options.Url.TrimEnd('/');
        var key = objectKey.Trim().TrimStart('/');
        // Encode path segments but keep '/' separators (matches supabase-js)
        var encodedKey = string.Join('/', key.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

        var signEndpoint =
            $"{baseUrl}/storage/v1/object/upload/sign/{_options.Bucket}/{encodedKey}";

        using var request = new HttpRequestMessage(HttpMethod.Post, signEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ServiceRoleKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Supabase signed upload URL request timed out for {ObjectKey}", objectKey);
            throw new InvalidOperationException("Timed out creating Supabase signed upload URL", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Supabase signed upload URL request failed for {ObjectKey}", objectKey);
            throw new InvalidOperationException("Failed to reach Supabase Storage", ex);
        }

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Supabase signed upload failed for {ObjectKey}: {StatusCode} {Body}",
                objectKey,
                (int)response.StatusCode,
                body);
            throw new InvalidOperationException(
                $"Supabase signed upload failed ({(int)response.StatusCode}): {Truncate(body, 500)}");
        }

        using var jsonDoc = JsonDocument.Parse(body);
        var root = jsonDoc.RootElement;
        string? relativeOrAbsolute = null;
        if (root.TryGetProperty("url", out var urlProp))
            relativeOrAbsolute = urlProp.GetString();
        else if (root.TryGetProperty("signedURL", out var signedProp))
            relativeOrAbsolute = signedProp.GetString();
        else if (root.TryGetProperty("signedUrl", out var signedUrlProp))
            relativeOrAbsolute = signedUrlProp.GetString();

        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            throw new InvalidOperationException("Supabase did not return a signed upload url");

        var uploadUrl = ToAbsoluteStorageUrl(baseUrl, relativeOrAbsolute);
        var publicUrl = $"{baseUrl}/storage/v1/object/public/{_options.Bucket}/{key}";

        _logger.LogInformation("Created signed upload URL for {ObjectKey}", objectKey);
        return (uploadUrl, publicUrl);
    }

    private static string ToAbsoluteStorageUrl(string supabaseBaseUrl, string url)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var path = url.StartsWith('/') ? url : "/" + url;
        // Storage API returns paths like /object/upload/sign/... relative to /storage/v1
        if (path.StartsWith("/storage/v1", StringComparison.OrdinalIgnoreCase))
            return $"{supabaseBaseUrl}{path}";

        return $"{supabaseBaseUrl}/storage/v1{path}";
    }

    private static string Truncate(string value, int max)
        => string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max] + "...";
}
