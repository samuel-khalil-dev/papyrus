namespace Papyrus.Application.Content;

public sealed record ContentListItem(Guid Id, string Slug, string Title, DateTimeOffset? PublishedAt, DateTimeOffset CreatedAt);

/// <summary>
/// Application-facing facade over <see cref="IContentQueryStore"/>.
/// </summary>
public sealed class ContentQueryHandlers(IContentQueryStore store)
{
    public Task<List<ContentListItem>> GetProjectsAsync(CancellationToken cancellationToken) =>
        store.GetProjectsAsync(cancellationToken);

    public Task<List<ContentListItem>> GetBlogPostsAsync(CancellationToken cancellationToken) =>
        store.GetBlogPostsAsync(cancellationToken);
}
