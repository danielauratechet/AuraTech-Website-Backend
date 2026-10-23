using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignDailyMetricConfiguration : IEntityTypeConfiguration<CampaignDailyMetric>
{
    public void Configure(EntityTypeBuilder<CampaignDailyMetric> builder)
    {
        builder.ToTable("CampaignDailyMetrics");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Impressions)
            .IsRequired();

        builder.Property(x => x.Clicks)
            .IsRequired();

        builder.Property(x => x.Spend)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.DailyMetrics)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CampaignId, x.Date })
            .IsUnique();

        builder.HasIndex(x => x.Date);
    }
}
