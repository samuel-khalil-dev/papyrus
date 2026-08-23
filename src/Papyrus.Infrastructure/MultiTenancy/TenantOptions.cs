namespace Papyrus.Infrastructure.MultiTenancy;

public sealed class TenantOptions
{
    public const string SectionName = "Tenant";

    public string DefaultTenantId { get; set; } = "main";
}
