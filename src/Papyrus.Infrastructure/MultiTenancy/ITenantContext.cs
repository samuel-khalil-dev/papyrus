namespace Papyrus.Infrastructure.MultiTenancy;

public interface ITenantContext
{
    string TenantId { get; }
}
