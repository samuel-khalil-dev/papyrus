using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => new { project.TenantId, project.Id });

        builder.Property(project => project.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(project => project.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Summary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasColumnType("text");

        builder.Property(project => project.RepositoryUrl)
            .HasMaxLength(2048);

        builder.Property(project => project.LiveUrl)
            .HasMaxLength(2048);

        builder.Property(project => project.CoverImageUrl)
            .HasMaxLength(2048);

        builder.Property(project => project.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(project => project.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(project => project.UpdatedBy).HasMaxLength(200);
        builder.Property(project => project.DeletedBy).HasMaxLength(200);
        builder.Property(project => project.DeleteReason).HasMaxLength(500);
        builder.Property(project => project.ConcurrencyToken).IsConcurrencyToken();

        builder.HasOne(project => project.Category)
            .WithMany(category => category.Projects)
            .HasPrincipalKey(category => new { category.TenantId, category.Id })
            .HasForeignKey(project => new { project.TenantId, project.CategoryId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(project => project.BlogPosts)
            .WithOne(post => post.Project)
            .HasPrincipalKey(project => new { project.TenantId, project.Id })
            .HasForeignKey(post => new { post.TenantId, post.ProjectId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(project => project.ProjectTags)
            .WithOne(projectTag => projectTag.Project)
            .HasPrincipalKey(project => new { project.TenantId, project.Id })
            .HasForeignKey(projectTag => new { projectTag.TenantId, projectTag.ProjectId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(project => new { project.TenantId, project.Slug })
            .IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(project => new { project.TenantId, project.IsPublished, project.IsFeatured, project.PublishedAt })
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(project => new { project.TenantId, project.CategoryId }).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(project => project.Title).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(project => project.Summary).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(project => project.Description).HasMethod("gin").HasOperators("gin_trgm_ops").HasFilter("\"IsDeleted\" = false");
        builder.ToTable(table => table.HasCheckConstraint("CK_Projects_TenantId_NotEmpty", "\"TenantId\" <> ''"));
    }
}
