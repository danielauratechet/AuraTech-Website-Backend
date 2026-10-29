namespace AuratechBackend.Data.Dtos.Responses;

public sealed class OrganizationProfileResponse
{
    public string AgencyName { get; init; } = string.Empty;

    public string? ContactEmail { get; init; }

    public string? PhysicalAddress { get; init; }

    public string? LogoUrl { get; init; }

    public string LanguagePreference { get; init; } = "en-US";
}
