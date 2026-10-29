using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CreateWalletWithdrawalRequest
{
    public decimal Amount { get; init; }

    public WalletPayoutMethod Method { get; init; }
}
