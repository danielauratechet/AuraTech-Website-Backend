namespace AuratechBackend.Data.Dtos.Requests;

public sealed class SetPasswordRequest
{
    public string Token { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
