using Papyrus.Core.Content;

namespace Papyrus.Application.Content;

public sealed record SaveCategoryCommand(Guid? Id, string Slug, string Name, string? Description, bool IsPublished);
public sealed record SaveTagCommand(Guid? Id, string Slug, string Name);

/// <summary>
/// Application-facing facade over <see cref="ITaxonomyCommandStore"/>.
/// </summary>
public sealed class TaxonomyCommandHandlers(ITaxonomyCommandStore store)
{
    public Task<CommandResult<ContentCategory>> SaveCategoryAsync(SaveCategoryCommand command, CancellationToken cancellationToken) =>
        store.SaveCategoryAsync(command, cancellationToken);

    public Task<CommandResult<ContentTag>> SaveTagAsync(SaveTagCommand command, CancellationToken cancellationToken) =>
        store.SaveTagAsync(command, cancellationToken);
}

/// <summary>
/// Application-facing facade over <see cref="IContentLifecycleStore"/>.
/// </summary>
public sealed class ContentLifecycleHandlers(IContentLifecycleStore store)
{
    public Task<bool> SoftDeleteCategoryAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        store.SoftDeleteCategoryAsync(id, reason, cancellationToken);

    public Task<bool> SoftDeleteTagAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        store.SoftDeleteTagAsync(id, reason, cancellationToken);

    public Task<bool> SoftDeleteProjectAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        store.SoftDeleteProjectAsync(id, reason, cancellationToken);

    public Task<bool> SoftDeleteBlogPostAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        store.SoftDeleteBlogPostAsync(id, reason, cancellationToken);

    public Task<bool> RestoreProjectAsync(Guid id, CancellationToken cancellationToken) =>
        store.RestoreProjectAsync(id, cancellationToken);

    public Task<bool> RestoreBlogPostAsync(Guid id, CancellationToken cancellationToken) =>
        store.RestoreBlogPostAsync(id, cancellationToken);
}
