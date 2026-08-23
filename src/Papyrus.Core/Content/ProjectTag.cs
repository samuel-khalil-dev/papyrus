using Papyrus.Core.MultiTenancy;

namespace Papyrus.Core.Content;

public sealed class ProjectTag : ITenantEntity
{
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public Guid TagId { get; set; }

    public ContentTag Tag { get; set; } = null!;

    public string TenantId { get; set; } = "main";
}
