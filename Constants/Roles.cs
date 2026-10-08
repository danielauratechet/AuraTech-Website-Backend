namespace AuratechBackend.Constants;

public static class Roles
{
    public const string PlatformOwner = "PlatformOwner";
    public const string PlatformAdmin = "PlatformAdmin";
    public const string PlatformOperator = "PlatformOperator";

    public const string CustomerOwner = "CustomerOwner";
    public const string CustomerAdmin = "CustomerAdmin";
    public const string CustomerUser = "CustomerUser";

    public static readonly string[] PlatformDefaults =
    [
        PlatformOwner,
        PlatformAdmin,
        PlatformOperator
    ];

    public static readonly string[] CustomerDefaults =
    [
        CustomerOwner,
        CustomerAdmin,
        CustomerUser
    ];
}
