using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class ContentTagConfiguration : IEntityTypeConfiguration<ContentTag>
{
    public void Configure(EntityTypeBuilder<ContentTag> builder)
    {
        builder.ToTable("ContentTags");

        builder.HasKey(tag => new { tag.TenantId, tag.Id });

        builder.Property(tag => tag.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(tag => tag.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tag => tag.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tag => tag.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tag => tag.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(tag => tag.UpdatedBy).HasMaxLength(200);
        builder.Property(tag => tag.DeletedBy).HasMaxLength(200);
        builder.Property(tag => tag.DeleteReason).HasMaxLength(500);
        builder.Property(tag => tag.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(tag => new { tag.TenantId, tag.Slug })
            .IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(tag => tag.Name)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.ToTable(table => table.HasCheckConstraint("CK_ContentTags_TenantId_NotEmpty", "\"TenantId\" <> ''"));
    }
}
