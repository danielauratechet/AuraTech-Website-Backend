using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data.Seed;

public static class CampaignTypeSeed
{
    private static readonly (string Code, string NameEn, string NameAm, string DescriptionEn, string DescriptionAm, string IconKey, int SortOrder)[] Defaults =
    [
        (
            "brand_awareness",
            "Brand Awareness",
            "የምርት ስም ግንዛቤ",
            "Maximize your reach and introduce your brand to potential customers across Ethiopia.",
            "ተደራሽነትዎን ያሳድጉ እና ምርትዎን በኢትዮጵያ ዙሪያ ላሉ ደንበኞች ያስተዋውቁ።",
            "megaphone",
            1
        ),
        (
            "lead_generation",
            "Lead Generation",
            "ደንበኞችን ማፍራት",
            "Drive high-quality inquiries and sign-ups for your services or products.",
            "ለአገልግሎቶችዎ ወይም ለምርቶችዎ ከፍተኛ ጥራት ያላቸውን ጥያቄዎች እና ምዝገባዎችን ያግኙ።",
            "user_plus",
            2
        ),
        (
            "website_traffic",
            "Website Traffic",
            "የድረ-ገጽ ጉብኝት",
            "Encourage more users to visit your landing pages and explore your digital presence.",
            "ተጠቃሚዎች ድረ-ገጽዎን እንዲጎበኙ እና ዲጂታል ገጽታዎን እንዲያዩ ያበረታቱ።",
            "globe",
            3
        ),
        (
            "store_visits",
            "Store Visits (Local)",
            "የሱቅ ጉብኝት",
            "Direct customers to your physical locations in major Ethiopian cities.",
            "ደንበኞችን በዋና ዋና የኢትዮጵያ ከተሞች ወደሚገኙት ሱቆችዎ ይምሩ።",
            "store",
            4
        )
    ];

    public static async Task EnsureAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var item in Defaults)
        {
            if (await db.CampaignTypes.AnyAsync(x => x.Code == item.Code, cancellationToken))
            {
                continue;
            }

            db.CampaignTypes.Add(new CampaignType
            {
                Code = item.Code,
                NameEn = item.NameEn,
                NameAm = item.NameAm,
                DescriptionEn = item.DescriptionEn,
                DescriptionAm = item.DescriptionAm,
                IconKey = item.IconKey,
                SortOrder = item.SortOrder,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
