using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
{
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.ToTable("BlogPostTags");

        builder.HasKey(blogPostTag => new { blogPostTag.TenantId, blogPostTag.BlogPostId, blogPostTag.TagId });

        builder.Property(blogPostTag => blogPostTag.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasOne(blogPostTag => blogPostTag.BlogPost)
            .WithMany(blogPost => blogPost.BlogPostTags)
            .HasPrincipalKey(post => new { post.TenantId, post.Id })
            .HasForeignKey(blogPostTag => new { blogPostTag.TenantId, blogPostTag.BlogPostId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(blogPostTag => blogPostTag.Tag)
            .WithMany(tag => tag.BlogPostTags)
            .HasPrincipalKey(tag => new { tag.TenantId, tag.Id })
            .HasForeignKey(blogPostTag => new { blogPostTag.TenantId, blogPostTag.TagId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(blogPostTag => new { blogPostTag.TenantId, blogPostTag.BlogPostId });
    }
}
