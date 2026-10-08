using Microsoft.AspNetCore.Identity;

namespace AuratechBackend.Data.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
