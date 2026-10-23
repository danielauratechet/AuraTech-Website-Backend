using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CurrentStep)
            .IsRequired();

        builder.Property(x => x.Languages)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.AgeRanges)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.Interests)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.DailyBudget)
            .HasPrecision(18, 2);

        builder.Property(x => x.Headline)
            .HasMaxLength(30);

        builder.Property(x => x.Description)
            .HasMaxLength(90);

        builder.Property(x => x.CreativeImageUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CampaignType)
            .WithMany()
            .HasForeignKey(x => x.CampaignTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.Status });
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
    }
}
