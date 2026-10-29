namespace AuratechBackend.Data.Dtos.Requests;

public sealed class WalletTransactionListQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
