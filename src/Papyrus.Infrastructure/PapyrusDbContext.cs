using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Papyrus.Infrastructure;

public sealed class PapyrusDbContext(
    DbContextOptions<PapyrusDbContext> options,
    IMultiTenantContextAccessor tenantContextAccessor)
    : IdentityDbContext(options), IMultiTenantDbContext
{
    public ITenantInfo? TenantInfo { get; } = tenantContextAccessor.MultiTenantContext?.TenantInfo;

    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureMultiTenant();
    }
}