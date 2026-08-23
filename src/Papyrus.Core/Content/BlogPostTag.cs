using Papyrus.Core.MultiTenancy;

namespace Papyrus.Core.Content;

public sealed class BlogPostTag : ITenantEntity
{
    public Guid BlogPostId { get; set; }

    public BlogPost BlogPost { get; set; } = null!;

    public Guid TagId { get; set; }

    public ContentTag Tag { get; set; } = null!;

    public string TenantId { get; set; } = "main";
}
