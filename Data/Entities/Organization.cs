using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? CompanyEmail { get; set; }

    public string? LogoUrl { get; set; }

    public string LanguagePreference { get; set; } = "en-US";

    public OrganizationType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsSelfManagedCampaign { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = [];

    public ICollection<ApplicationRole> Roles { get; set; } = [];
}
