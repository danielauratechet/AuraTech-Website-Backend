namespace AuratechBackend.Constants;

public static class AuthPolicies
{
    public const string UsersManage = "Permission:users.manage";
    public const string UsersRead = "Permission:users.read";
    public const string RolesManage = "Permission:roles.manage";
    public const string PlatformOrganization = "OrganizationType:Platform";
    public const string CustomerOrganization = "OrganizationType:Customer";

    public const string KycRead = "Permission:kyc.read";
    public const string KycWrite = "Permission:kyc.write";
    public const string KycUpdate = "Permission:kyc.update";
    public const string KycView = "Permission:kyc.view";
    public const string KycApprove = "Permission:kyc.approve";
    public const string KycReject = "Permission:kyc.reject";
}
