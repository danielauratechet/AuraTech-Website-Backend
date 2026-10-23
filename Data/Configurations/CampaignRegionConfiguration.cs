using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignRegionConfiguration : IEntityTypeConfiguration<CampaignRegion>
{
    public void Configure(EntityTypeBuilder<CampaignRegion> builder)
    {
        builder.ToTable("CampaignRegions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Mode)
            .IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Regions)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CampaignId, x.RegionId })
            .IsUnique();
    }
}
