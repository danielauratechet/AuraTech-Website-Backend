namespace AuratechBackend.Data.Dtos.Requests;

public sealed class ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}
