using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data.Seed;

public static class OrganizationRoleBootstrap
{
    public static async Task EnsureDefaultRolesAsync(
        AppDbContext db,
        Organization organization,
        CancellationToken cancellationToken = default)
    {
        var roleNames = organization.Type == OrganizationType.Platform
            ? Roles.PlatformDefaults
            : Roles.CustomerDefaults;

        var permissionsByCode = await db.Permissions
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var roleName in roleNames)
        {
            var role = await db.Roles
                .FirstOrDefaultAsync(r => r.OrganizationId == organization.Id && r.Name == roleName, cancellationToken);

            if (role is null)
            {
                role = new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = IdentitySeed.NormalizeOrgRoleName(organization.Id, roleName),
                    OrganizationId = organization.Id,
                    Description = $"{roleName} role for {organization.Type} organization",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                db.Roles.Add(role);
                await db.SaveChangesAsync(cancellationToken);
            }

            foreach (var permissionCode in GetRolePermissions(organization.Type, roleName))
            {
                if (!permissionsByCode.TryGetValue(permissionCode, out var permission))
                {
                    continue;
                }

                var exists = await db.RolePermissions.AnyAsync(
                    rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id,
                    cancellationToken);

                if (exists)
                {
                    continue;
                }

                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static IEnumerable<string> GetRolePermissions(OrganizationType orgType, string roleName)
    {
        if (orgType == OrganizationType.Platform)
        {
            return roleName switch
            {
                Roles.PlatformOwner => PermissionCodes.Catalog.Select(p => p.Code),
                Roles.PlatformAdmin =>
                [
                    PermissionCodes.UsersRead,
                    PermissionCodes.UsersManage,
                    PermissionCodes.RolesManage,
                    PermissionCodes.OrgManage,
                    PermissionCodes.KycView,
                    PermissionCodes.KycApprove,
                    PermissionCodes.KycReject,
                    PermissionCodes.CampaignTypesRead,
                    PermissionCodes.CampaignTypesManage,
                    PermissionCodes.RegionsRead,
                    PermissionCodes.RegionsManage
                ],
                Roles.PlatformOperator =>
                [
                    PermissionCodes.UsersRead,
                    PermissionCodes.KycView,
                    PermissionCodes.CampaignTypesRead,
                    PermissionCodes.RegionsRead
                ],
                _ => []
            };
        }

        return roleName switch
        {
            Roles.CustomerOwner or Roles.CustomerAdmin =>
            [
                PermissionCodes.UsersRead,
                PermissionCodes.UsersManage,
                PermissionCodes.RolesManage,
                PermissionCodes.OrgManage,
                PermissionCodes.KycRead,
                PermissionCodes.KycWrite,
                PermissionCodes.KycUpdate,
                PermissionCodes.CampaignTypesRead,
                PermissionCodes.RegionsRead
            ],
            Roles.CustomerUser =>
            [
                PermissionCodes.UsersRead,
                PermissionCodes.KycRead,
                PermissionCodes.CampaignTypesRead,
                PermissionCodes.RegionsRead
            ],
            _ => []
        };
    }
}
