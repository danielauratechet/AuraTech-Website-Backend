namespace AuratechBackend.Services.Payments;

public interface IStarpayClient
{
    Task<StarpayCreateOrderResult> CreateOrderAsync(StarpayCreateOrderCommand command, CancellationToken cancellationToken = default);

    Task<StarpayVerifyResult> VerifyAsync(string orderId, CancellationToken cancellationToken = default);
}

public sealed class StarpayCreateOrderCommand
{
    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public string CustomerPhoneNumber { get; init; } = string.Empty;

    public string CallbackUrl { get; init; } = string.Empty;

    public string? RedirectUrl { get; init; }
}

public sealed class StarpayCreateOrderResult
{
    public bool Succeeded { get; init; }

    public string? BillRefNo { get; init; }

    public string? PaymentUrl { get; init; }

    public string? ErrorMessage { get; init; }

    public static StarpayCreateOrderResult Success(string billRefNo, string paymentUrl)
        => new() { Succeeded = true, BillRefNo = billRefNo, PaymentUrl = paymentUrl };

    public static StarpayCreateOrderResult Failure(string message)
        => new() { Succeeded = false, ErrorMessage = message };
}

public sealed class StarpayVerifyResult
{
    public bool CallFailed { get; init; }

    public string? Status { get; init; }

    public decimal? Amount { get; init; }

    public string? ErrorMessage { get; init; }

    public bool IsPaid => IsPaidStatus(Status);

    public bool IsFailed => IsFailedStatus(Status);

    public static bool IsPaidStatus(string? status)
        => string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase);

    public static bool IsFailedStatus(string? status)
        => string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "EXPIRED", StringComparison.OrdinalIgnoreCase);
}
