using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Core.MultiTenancy;
using Papyrus.Core.Persistence;
using Papyrus.Infrastructure;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// EF Core adapter for <see cref="IContentLifecycleStore"/>.
/// </summary>
public sealed class EfContentLifecycleStore(PapyrusDbContext db, TimeProvider timeProvider) : IContentLifecycleStore
{
    public Task<bool> SoftDeleteCategoryAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.ContentCategories, id, true, reason, cancellationToken);

    public Task<bool> SoftDeleteTagAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.ContentTags, id, true, reason, cancellationToken);

    public Task<bool> SoftDeleteProjectAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.Projects, id, true, reason, cancellationToken);

    public Task<bool> SoftDeleteBlogPostAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.BlogPosts, id, true, reason, cancellationToken);

    public Task<bool> RestoreProjectAsync(Guid id, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.Projects, id, false, null, cancellationToken);

    public Task<bool> RestoreBlogPostAsync(Guid id, CancellationToken cancellationToken) =>
        SetDeletedStateAsync(db.BlogPosts, id, false, null, cancellationToken);

    private async Task<bool> SetDeletedStateAsync<TEntity>(
        DbSet<TEntity> set,
        Guid id,
        bool deleted,
        string? reason,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity, ITenantEntity, ISoftDeletable
    {
        // IgnoreQueryFilters is required because deleted rows are excluded by the
        // global soft-delete filter; without it, an entity to restore or re-soft-delete
        // would never be found.
        var entity = await set.IgnoreQueryFilters().FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == db.TenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        // Lifecycle transitions are applied directly so the behavior does not depend on
        // the save-changes interceptor being registered. Restores clear the soft-delete
        // fields so the row becomes visible through the default filter again.
        var now = timeProvider.GetUtcNow();
        entity.IsDeleted = deleted;
        entity.DeletedAt = deleted ? now : null;
        entity.DeletedBy = null;
        entity.DeleteReason = deleted ? reason : null;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
