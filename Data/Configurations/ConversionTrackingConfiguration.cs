using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class ConversionTrackingConfiguration : IEntityTypeConfiguration<ConversionTracking>
{
    public void Configure(EntityTypeBuilder<ConversionTracking> builder)
    {
        builder.ToTable("ConversionTrackings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.ConversionType)
            .IsRequired();

        builder.Property(x => x.GoalName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CountMode)
            .IsRequired();

        builder.Property(x => x.ConversionValueEtb)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ClickThroughWindowDays)
            .IsRequired();

        builder.Property(x => x.InstallMethod)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.TrackingCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId)
            .IsUnique();

        builder.HasIndex(x => x.TrackingCode)
            .IsUnique();

        builder.HasOne(x => x.Organization)
            .WithOne()
            .HasForeignKey<ConversionTracking>(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
