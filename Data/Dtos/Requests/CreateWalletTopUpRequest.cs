namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CreateWalletTopUpRequest
{
    public decimal Amount { get; init; }

    public string? CustomerName { get; init; }

    public string CustomerPhoneNumber { get; init; } = string.Empty;

    public string? CustomerEmail { get; init; }

    public string? ReturnUrl { get; init; }
}
