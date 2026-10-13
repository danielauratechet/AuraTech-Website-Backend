using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data.Seed;

public static class RegionSeed
{
    private static readonly (string Code, string NameEn, string NameAm, long EstimatedUsers, int SortOrder)[] Defaults =
    [
        ("AA", "Addis Ababa", "አዲስ አበባ", 4_200_000, 1),
        ("OR", "Oromia Region", "ኦሮሚያ ክልል", 2_800_000, 2),
        ("AM", "Amhara Region", "አማራ ክልል", 1_900_000, 3),
        ("TI", "Tigray Region", "ትግራይ ክልል", 850_000, 4),
        ("SI", "Sidama Region", "ሲዳማ ክልል", 780_000, 5),
        ("SO", "Somali Region", "ሶማሌ ክልል", 650_000, 6),
        ("DD", "Dire Dawa", "ድሬዳዋ", 490_000, 7),
        ("AF", "Afar Region", "አፋር ክልል", 320_000, 8),
        ("BG", "Benishangul-Gumuz", "ቤንሻንጉል ጉሙዝ", 240_000, 9),
        ("HA", "Harari Region", "ሐረሪ ክልል", 210_000, 10),
        ("GA", "Gambela Region", "ጋምቤላ ክልል", 180_000, 11),
        ("SW", "South West Ethiopia", "ደቡብ ምዕራብ ኢትዮጵያ", 310_000, 12),
        ("CE", "Central Ethiopia", "ማዕከላዊ ኢትዮጵያ", 450_000, 13),
        ("SE", "South Ethiopia", "ደቡብ ኢትዮጵያ", 520_000, 14)
    ];

    public static async Task EnsureAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var item in Defaults)
        {
            if (await db.Regions.AnyAsync(x => x.Code == item.Code, cancellationToken))
            {
                continue;
            }

            db.Regions.Add(new Region
            {
                Code = item.Code,
                NameEn = item.NameEn,
                NameAm = item.NameAm,
                EstimatedUsers = item.EstimatedUsers,
                SortOrder = item.SortOrder,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
