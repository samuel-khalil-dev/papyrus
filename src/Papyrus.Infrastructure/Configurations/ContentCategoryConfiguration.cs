using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class ContentCategoryConfiguration : IEntityTypeConfiguration<ContentCategory>
{
    public void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.ToTable("ContentCategories");

        builder.HasKey(category => new { category.TenantId, category.Id });

        builder.Property(category => category.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(category => category.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(category => category.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasColumnType("text");

        builder.Property(category => category.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(category => category.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(category => category.UpdatedBy).HasMaxLength(200);
        builder.Property(category => category.DeletedBy).HasMaxLength(200);
        builder.Property(category => category.DeleteReason).HasMaxLength(500);
        builder.Property(category => category.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(category => new { category.TenantId, category.Slug })
            .IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(category => new { category.TenantId, category.IsPublished }).HasFilter("\"IsDeleted\" = false");
        builder.ToTable(table => table.HasCheckConstraint("CK_ContentCategories_TenantId_NotEmpty", "\"TenantId\" <> ''"));
    }
}
