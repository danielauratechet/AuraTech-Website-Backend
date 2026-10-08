using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public OrganizationType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = [];

    public ICollection<ApplicationRole> Roles { get; set; } = [];
}
