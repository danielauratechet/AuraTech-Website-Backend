namespace AuratechBackend.Constants;

public static class AuditActionCodes
{
    public const string CampaignLaunched = "campaign.launched";
    public const string CampaignPaused = "campaign.paused";
    public const string CampaignResumed = "campaign.resumed";

    public const string UserInvited = "user.invited";
    public const string UserUpdated = "user.updated";
    public const string UserDeactivated = "user.deactivated";
    public const string UserActivated = "user.activated";

    public const string RoleCreated = "role.created";

    public const string KycSubmitted = "kyc.submitted";
    public const string KycUpdated = "kyc.updated";
    public const string KycApproved = "kyc.approved";
    public const string KycRejected = "kyc.rejected";
}
