using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.HasKey(post => new { post.TenantId, post.Id });

        builder.Property(post => post.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(post => post.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(post => post.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(post => post.Summary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(post => post.ContentMarkdown)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(post => post.CoverImageUrl)
            .HasMaxLength(2048);

        builder.Property(post => post.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(post => post.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(post => post.UpdatedBy).HasMaxLength(200);
        builder.Property(post => post.DeletedBy).HasMaxLength(200);
        builder.Property(post => post.DeleteReason).HasMaxLength(500);
        builder.Property(post => post.ConcurrencyToken).IsConcurrencyToken();

        builder.HasOne(post => post.Project)
            .WithMany(project => project.BlogPosts)
            .HasPrincipalKey(project => new { project.TenantId, project.Id })
            .HasForeignKey(post => new { post.TenantId, post.ProjectId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(post => post.Category)
            .WithMany(category => category.BlogPosts)
            .HasPrincipalKey(category => new { category.TenantId, category.Id })
            .HasForeignKey(post => new { post.TenantId, post.CategoryId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(post => post.BlogPostTags)
            .WithOne(blogPostTag => blogPostTag.BlogPost)
            .HasPrincipalKey(post => new { post.TenantId, post.Id })
            .HasForeignKey(blogPostTag => new { blogPostTag.TenantId, blogPostTag.BlogPostId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(post => new { post.TenantId, post.Slug })
            .IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(post => new { post.TenantId, post.IsPublished, post.PublishedAt }).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(post => new { post.TenantId, post.CategoryId }).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(post => new { post.TenantId, post.ProjectId }).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(post => post.Title).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(post => post.Summary).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(post => post.ContentMarkdown).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.ToTable(table => table.HasCheckConstraint("CK_BlogPosts_TenantId_NotEmpty", "\"TenantId\" <> ''"));
    }
}
