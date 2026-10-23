using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignRegionDailyMetricConfiguration : IEntityTypeConfiguration<CampaignRegionDailyMetric>
{
    public void Configure(EntityTypeBuilder<CampaignRegionDailyMetric> builder)
    {
        builder.ToTable("CampaignRegionDailyMetrics");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Impressions)
            .IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.RegionDailyMetrics)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CampaignId, x.Date, x.RegionId })
            .IsUnique();

        builder.HasIndex(x => x.Date);
    }
}
