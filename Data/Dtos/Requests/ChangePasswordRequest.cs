namespace AuratechBackend.Data.Dtos.Requests;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}
