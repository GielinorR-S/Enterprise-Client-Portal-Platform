using HelixPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelixPortal.Infrastructure.Data.Configurations;

public class ClientOrganisationConfiguration : IEntityTypeConfiguration<ClientOrganisation>
{
    public void Configure(EntityTypeBuilder<ClientOrganisation> builder)
    {
        builder.ToTable("ClientOrganisations");

        builder.HasKey(co => co.Id);

        builder.Property(co => co.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(co => co.Address)
            .HasMaxLength(500);

        builder.Property(co => co.Timezone)
            .HasMaxLength(50);

        builder.Property(co => co.IsActive)
            .IsRequired();

        builder.Property(co => co.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(co => co.PrimaryContact)
            .WithMany()
            .HasForeignKey(co => co.PrimaryContactId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

