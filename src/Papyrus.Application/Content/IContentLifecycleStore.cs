namespace Papyrus.Application.Content;

/// <summary>
/// Application-owned port for content lifecycle (soft-delete/restore) operations.
/// </summary>
public interface IContentLifecycleStore
{
    Task<bool> SoftDeleteCategoryAsync(Guid id, string reason, CancellationToken cancellationToken);

    Task<bool> SoftDeleteTagAsync(Guid id, string reason, CancellationToken cancellationToken);

    Task<bool> SoftDeleteProjectAsync(Guid id, string reason, CancellationToken cancellationToken);

    Task<bool> SoftDeleteBlogPostAsync(Guid id, string reason, CancellationToken cancellationToken);

    Task<bool> RestoreProjectAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> RestoreBlogPostAsync(Guid id, CancellationToken cancellationToken);
}
