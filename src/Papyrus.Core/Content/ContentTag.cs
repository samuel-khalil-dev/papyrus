using Papyrus.Core.MultiTenancy;
using Papyrus.Core.Persistence;

namespace Papyrus.Core.Content;

public sealed class ContentTag : IEntity, ITenantEntity, IAuditedEntity, ISoftDeletable, IContentSlugged
{
    public Guid Id { get; set; }

    public string TenantId { get; set; } = "main";

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();

    public ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
}
