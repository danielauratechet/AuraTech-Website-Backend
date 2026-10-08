using Microsoft.AspNetCore.Identity;

namespace AuratechBackend.Data.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
