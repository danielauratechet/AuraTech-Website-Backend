using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class WalletTransactionResponse
{
    public int Id { get; init; }

    public string Reference { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public WalletTransactionDirection Direction { get; init; }

    public WalletTransactionKind Kind { get; init; }

    public WalletTransactionStatus Status { get; init; }

    public string? Method { get; init; }

    public decimal Amount { get; init; }

    public decimal SignedAmount { get; init; }

    public decimal? BalanceAfter { get; init; }

    public string Currency { get; init; } = OrganizationWalletResponse.CurrencyCode;

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? CompletedAtUtc { get; init; }
}

public sealed class WalletWithdrawalResponse
{
    public int Id { get; init; }

    public string Reference { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = OrganizationWalletResponse.CurrencyCode;

    public WalletTransactionStatus Status { get; init; }

    public string? Method { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? CompletedAtUtc { get; init; }
}

public sealed class OrganizationWalletResponse
{
    public const string CurrencyCode = "ETB";

    public decimal AvailableBalance { get; init; }

    public string Currency { get; init; } = CurrencyCode;

    public WalletWithdrawalResponse? PendingWithdrawal { get; init; }

    public WalletWithdrawalResponse? LastCompletedWithdrawal { get; init; }

    public IReadOnlyList<WalletTransactionResponse> RecentTransactions { get; init; } = [];
}

public sealed class WalletTopUpResponse
{
    public int TransactionId { get; init; }

    public string Reference { get; init; } = string.Empty;

    public string? BillRefNo { get; init; }

    public string? PaymentUrl { get; init; }

    public WalletTransactionStatus Status { get; init; }

    public decimal Amount { get; init; }

    public string Currency { get; init; } = OrganizationWalletResponse.CurrencyCode;

    public decimal AvailableBalance { get; init; }
}
