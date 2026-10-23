using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignDeviceDailyMetricConfiguration : IEntityTypeConfiguration<CampaignDeviceDailyMetric>
{
    public void Configure(EntityTypeBuilder<CampaignDeviceDailyMetric> builder)
    {
        builder.ToTable("CampaignDeviceDailyMetrics");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Device)
            .IsRequired();

        builder.Property(x => x.Impressions)
            .IsRequired();

        builder.Property(x => x.Clicks)
            .IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.DeviceDailyMetrics)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CampaignId, x.Date, x.Device })
            .IsUnique();

        builder.HasIndex(x => x.Date);
    }
}
