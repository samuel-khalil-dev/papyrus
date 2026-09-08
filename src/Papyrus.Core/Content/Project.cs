using Papyrus.Core.MultiTenancy;
using Papyrus.Core.Persistence;

namespace Papyrus.Core.Content;

public sealed class Project : IEntity, ITenantEntity, IAuditedEntity, ISoftDeletable
{
    public Guid Id { get; set; }

    public string TenantId { get; set; } = "main";

    public Guid? CategoryId { get; set; }

    public ContentCategory? Category { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RepositoryUrl { get; set; }

    public string? LiveUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
