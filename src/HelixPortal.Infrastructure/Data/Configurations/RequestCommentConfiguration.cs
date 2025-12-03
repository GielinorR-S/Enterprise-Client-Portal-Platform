using HelixPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelixPortal.Infrastructure.Data.Configurations;

public class RequestCommentConfiguration : IEntityTypeConfiguration<RequestComment>
{
    public void Configure(EntityTypeBuilder<RequestComment> builder)
    {
        builder.ToTable("RequestComments");

        builder.HasKey(rc => rc.Id);

        builder.Property(rc => rc.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(rc => rc.IsInternal)
            .IsRequired();

        builder.Property(rc => rc.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(rc => rc.Request)
            .WithMany(r => r.Comments)
            .HasForeignKey(rc => rc.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.AuthorUser)
            .WithMany(u => u.Comments)
            .HasForeignKey(rc => rc.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(rc => rc.RequestId);
        builder.HasIndex(rc => rc.CreatedAt);
    }
}

