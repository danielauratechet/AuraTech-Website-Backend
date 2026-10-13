using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignTypeConfiguration : IEntityTypeConfiguration<CampaignType>
{
    public void Configure(EntityTypeBuilder<CampaignType> builder)
    {
        builder.ToTable("CampaignTypes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Code)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.NameEn)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NameAm)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.DescriptionAm)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.IconKey)
            .HasMaxLength(64);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => new { x.IsActive, x.SortOrder });
    }
}
