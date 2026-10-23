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
    public const string KycPresign = "Permission:kyc.presign";
    public const string KycView = "Permission:kyc.view";
    public const string KycApprove = "Permission:kyc.approve";
    public const string KycReject = "Permission:kyc.reject";

    public const string CampaignTypesRead = "Permission:campaign-types.read";
    public const string CampaignTypesManage = "Permission:campaign-types.manage";
    public const string RegionsRead = "Permission:regions.read";
    public const string RegionsManage = "Permission:regions.manage";

    public const string CampaignsRead = "Permission:campaigns.read";
    public const string CampaignsWrite = "Permission:campaigns.write";
    public const string CampaignsLaunch = "Permission:campaigns.launch";
    public const string CampaignsPresign = "Permission:campaigns.presign";

    public const string AuditLogsRead = "Permission:audit-logs.read";
}
