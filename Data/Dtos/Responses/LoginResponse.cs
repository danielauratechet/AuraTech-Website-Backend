namespace AuratechBackend.Data.Dtos.Responses;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; init; }
}
