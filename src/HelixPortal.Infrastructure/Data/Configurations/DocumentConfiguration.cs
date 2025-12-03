using HelixPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelixPortal.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.BlobStoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.VersionNumber)
            .IsRequired();

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(d => d.ClientOrganisation)
            .WithMany(co => co.Documents)
            .HasForeignKey(d => d.ClientOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.UploadedByUser)
            .WithMany(u => u.UploadedDocuments)
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => d.ClientOrganisationId);
        builder.HasIndex(d => d.UploadedAt);
    }
}

