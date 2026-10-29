using System.Security.Cryptography;
using System.Text;

namespace AuratechBackend.Services.Payments;

public static class StarpaySignature
{
    public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(5);

    public static bool IsValid(string rawBody, string? timestamp, string? signature, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(timestamp)
            || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var message = $"{timestamp}.{rawBody}";
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));
        var expected = Convert.ToHexString(hash);

        try
        {
            var provided = Convert.FromHexString(signature.Trim());
            var expectedBytes = Convert.FromHexString(expected);
            return provided.Length == expectedBytes.Length
                   && CryptographicOperations.FixedTimeEquals(provided, expectedBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool IsFresh(string? timestamp, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(timestamp) || !long.TryParse(timestamp, out var value))
        {
            return false;
        }

        DateTimeOffset instant;
        try
        {
            instant = timestamp.Length >= 13
                ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                : DateTimeOffset.FromUnixTimeSeconds(value);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        return (utcNow - instant).Duration() <= MaxAge;
    }
}
