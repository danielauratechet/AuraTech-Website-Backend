namespace AuratechBackend.Core.Enums;

public enum WalletPayoutMethod
{
    Telebirr = 0,
    CbeBirr = 1,
    HibretBank = 2
}

public static class WalletPayoutMethodNames
{
    public static string ToDisplayName(this WalletPayoutMethod method)
        => method switch
        {
            WalletPayoutMethod.Telebirr => "Telebirr",
            WalletPayoutMethod.CbeBirr => "CBE Birr",
            WalletPayoutMethod.HibretBank => "Hibret Bank",
            _ => method.ToString()
        };
}
