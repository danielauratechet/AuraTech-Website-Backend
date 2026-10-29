using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.CompanyEmail)
            .HasMaxLength(256);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.LanguagePreference)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("en-US");

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.IsSelfManagedCampaign)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.Type, x.Name });
    }
}
