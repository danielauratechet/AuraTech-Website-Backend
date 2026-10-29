namespace AuratechBackend.Data.Dtos.Requests;

public sealed class UpdateOrganizationProfileRequest
{
    public string AgencyName { get; init; } = string.Empty;

    public string? ContactEmail { get; init; }

    public string? PhysicalAddress { get; init; }

    public string? LogoUrl { get; init; }

    public string LanguagePreference { get; init; } = string.Empty;
}
