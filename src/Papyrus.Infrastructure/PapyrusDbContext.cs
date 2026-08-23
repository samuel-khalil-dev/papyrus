using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Core.MultiTenancy;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Infrastructure;

public sealed class PapyrusDbContext(DbContextOptions<PapyrusDbContext> options, ITenantContext? tenantContext = null)
    : DbContext(options)
{
    private readonly ITenantContext tenantContext = tenantContext ?? new TenantContext("main");

    public string TenantId => tenantContext.TenantId;

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();

    public DbSet<ContentTag> ContentTags => Set<ContentTag>();

    public DbSet<ProjectTag> ProjectTags => Set<ProjectTag>();

    public DbSet<BlogPostTag> BlogPostTags => Set<BlogPostTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PapyrusDbContext).Assembly);

        modelBuilder.Entity<Project>().HasQueryFilter(entity => entity.TenantId == TenantId && !entity.IsDeleted);
        modelBuilder.Entity<BlogPost>().HasQueryFilter(entity => entity.TenantId == TenantId && !entity.IsDeleted);
        modelBuilder.Entity<ContentCategory>().HasQueryFilter(entity => entity.TenantId == TenantId && !entity.IsDeleted);
        modelBuilder.Entity<ContentTag>().HasQueryFilter(entity => entity.TenantId == TenantId && !entity.IsDeleted);
        modelBuilder.Entity<ProjectTag>().HasQueryFilter(entity => entity.TenantId == TenantId);
        modelBuilder.Entity<BlogPostTag>().HasQueryFilter(entity => entity.TenantId == TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceTenantBoundary();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnforceTenantBoundary();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnforceTenantBoundary()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = TenantId;
                continue;
            }

            var tenantId = entry.Entity.TenantId;
            if (!string.Equals(tenantId, TenantId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The entity belongs to a different tenant.");
            }
        }
    }
}
