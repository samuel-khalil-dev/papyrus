using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Configurations;

internal sealed class ProjectTagConfiguration : IEntityTypeConfiguration<ProjectTag>
{
    public void Configure(EntityTypeBuilder<ProjectTag> builder)
    {
        builder.ToTable("ProjectTags");

        builder.HasKey(projectTag => new { projectTag.TenantId, projectTag.ProjectId, projectTag.TagId });

        builder.Property(projectTag => projectTag.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasOne(projectTag => projectTag.Project)
            .WithMany(project => project.ProjectTags)
            .HasPrincipalKey(project => new { project.TenantId, project.Id })
            .HasForeignKey(projectTag => new { projectTag.TenantId, projectTag.ProjectId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(projectTag => projectTag.Tag)
            .WithMany(tag => tag.ProjectTags)
            .HasPrincipalKey(tag => new { tag.TenantId, tag.Id })
            .HasForeignKey(projectTag => new { projectTag.TenantId, projectTag.TagId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(projectTag => new { projectTag.TenantId, projectTag.ProjectId });
    }
}
