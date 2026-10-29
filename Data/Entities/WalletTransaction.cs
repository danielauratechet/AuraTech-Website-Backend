using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class WalletTransaction
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public OrganizationWallet Wallet { get; set; } = null!;

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public WalletTransactionKind Kind { get; set; }

    public WalletTransactionDirection Direction { get; set; }

    public WalletTransactionStatus Status { get; set; }

    public decimal Amount { get; set; }

    public decimal? BalanceAfter { get; set; }

    public string Currency { get; set; } = OrganizationWallet.CurrencyCode;

    public string Description { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string? Method { get; set; }

    public string? StarpayBillRefNo { get; set; }

    public string? PaymentUrl { get; set; }

    public int CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }
}
