namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CustomerRegisterRequest
{
    public string OrganizationName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public bool IsSelfManagedCampaign { get; init; }
}
