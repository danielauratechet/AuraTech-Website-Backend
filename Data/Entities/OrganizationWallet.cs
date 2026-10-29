namespace AuratechBackend.Data.Entities;

public class OrganizationWallet
{
    public const string CurrencyCode = "ETB";

    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public decimal AvailableBalance { get; set; }

    public string Currency { get; set; } = CurrencyCode;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<WalletTransaction> Transactions { get; set; } = [];
}
