using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AuratechBackend.Helpers;

public static partial class SensitiveDataRedactor
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "pwd",
        "secret",
        "token",
        "accessToken",
        "refreshToken",
        "authorization",
        "apiKey",
        "apikey",
        "clientSecret",
        "creditCard",
        "cardNumber",
        "cvv",
        "ssn"
    };

    public static string Redact(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content ?? string.Empty;
        }

        try
        {
            var node = JsonNode.Parse(content);
            if (node is null)
            {
                return AuthorizationHeaderRegex().Replace(content, "$1***REDACTED***");
            }

            RedactNode(node);
            return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            return AuthorizationHeaderRegex().Replace(content, "$1***REDACTED***");
        }
    }

    private static void RedactNode(JsonNode node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var property in obj.ToList())
                {
                    if (SensitiveKeys.Contains(property.Key))
                    {
                        obj[property.Key] = "***REDACTED***";
                        continue;
                    }

                    if (property.Value is not null)
                    {
                        RedactNode(property.Value);
                    }
                }

                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        RedactNode(item);
                    }
                }

                break;
        }
    }

    [GeneratedRegex(@"(authorization\s*[:=]\s*)([^\s,;}]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AuthorizationHeaderRegex();
}
