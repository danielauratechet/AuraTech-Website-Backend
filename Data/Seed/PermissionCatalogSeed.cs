using AuratechBackend.Constants;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data.Seed;

public static class PermissionCatalogSeed
{
    public static async Task EnsureAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var (code, name, description) in PermissionCodes.Catalog)
        {
            if (await db.Permissions.AnyAsync(p => p.Code == code, cancellationToken))
            {
                continue;
            }

            db.Permissions.Add(new Permission
            {
                Code = code,
                Name = name,
                Description = description
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
