namespace AuratechBackend.Constants;

public static class PermissionCodes
{
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string OrgManage = "org.manage";
    public const string PlatformManage = "platform.manage";

    public const string KycRead = "kyc.read";
    public const string KycWrite = "kyc.write";
    public const string KycUpdate = "kyc.update";
    public const string KycView = "kyc.view";
    public const string KycApprove = "kyc.approve";
    public const string KycReject = "kyc.reject";

    public static readonly IReadOnlyList<(string Code, string Name, string Description)> Catalog =
    [
        (UsersRead, "Read users", "View users within the organization"),
        (UsersManage, "Manage users", "Create, update, and deactivate users"),
        (RolesManage, "Manage roles", "Assign roles and permissions"),
        (OrgManage, "Manage organization", "Update organization settings"),
        (PlatformManage, "Manage platform", "Platform-wide administration"),
        (KycRead, "Read KYC", "View own organization KYC submission"),
        (KycWrite, "Submit KYC", "Create initial KYC submission"),
        (KycUpdate, "Update KYC", "Update or resubmit KYC documents"),
        (KycView, "View KYC", "View customer KYC submissions (platform)"),
        (KycApprove, "Approve KYC", "Approve customer KYC submissions"),
        (KycReject, "Reject KYC", "Reject customer KYC submissions with a comment")
    ];
}
