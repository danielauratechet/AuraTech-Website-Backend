using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data.Seed;

public static class IdentitySeed
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeed");

        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied.");
    }

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeed");

        await PermissionCatalogSeed.EnsureAsync(db, cancellationToken);

        var platformOrgName = configuration["Seed:PlatformOrganizationName"] ?? "AuraTech Platform";
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@auratech.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "ChangeMe!123";

        var platformOrg = await db.Organizations
            .FirstOrDefaultAsync(o => o.Type == OrganizationType.Platform, cancellationToken);

        if (platformOrg is null)
        {
            platformOrg = new Organization
            {
                Name = platformOrgName,
                Type = OrganizationType.Platform,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            db.Organizations.Add(platformOrg);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded Platform organization '{Name}'.", platformOrg.Name);
        }

        await OrganizationRoleBootstrap.EnsureDefaultRolesAsync(db, platformOrg, cancellationToken);

        var customerOrgs = await db.Organizations
            .Where(o => o.Type == OrganizationType.Customer)
            .ToListAsync(cancellationToken);

        foreach (var customerOrg in customerOrgs)
        {
            await OrganizationRoleBootstrap.EnsureDefaultRolesAsync(db, customerOrg, cancellationToken);
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Platform",
                LastName = "Admin",
                OrganizationId = platformOrg.Id,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create seed admin: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
            }

            var ownerRole = await db.Roles.SingleAsync(
                r => r.OrganizationId == platformOrg.Id && r.Name == Roles.PlatformOwner,
                cancellationToken);

            db.UserRoles.Add(new IdentityUserRole<int>
            {
                UserId = adminUser.Id,
                RoleId = ownerRole.Id
            });
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded Platform admin user '{Email}'.", adminEmail);
        }
    }

    public static string NormalizeOrgRoleName(int organizationId, string roleName)
        => $"{organizationId}:{roleName}".ToUpperInvariant();
}
