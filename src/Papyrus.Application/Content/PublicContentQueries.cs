using Papyrus.Contracts.Content;

namespace Papyrus.Application.Content;

/// <summary>
/// Application-facing facade over <see cref="IPublicContentStore"/>. Keeps the
/// public query surface stable while delegating persistence to the injected
/// store adapter, so the application layer never depends on EF/PostgreSQL.
/// </summary>
public sealed class PublicContentQueryHandlers(IPublicContentStore store)
{
    public Task<PublicBlogIndexResult> GetBlogIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken) =>
        store.GetBlogIndexAsync(searchTerm, tag, now, page, pageSize, cancellationToken);

    public Task<PublicPostPageResult?> GetPostPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken) =>
        store.GetPostPageAsync(slug, now, cancellationToken);

    public Task<PublicPortfolioIndexResult> GetPortfolioIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken) =>
        store.GetPortfolioIndexAsync(searchTerm, tag, now, page, pageSize, cancellationToken);

    public Task<PublicProjectPageResult?> GetProjectPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken) =>
        store.GetProjectPageAsync(slug, now, cancellationToken);

    public Task<PublicProjectDetail?> GetProjectAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken) =>
        store.GetProjectAsync(slug, now, cancellationToken);

    public Task<PublicPostDetail?> GetPostAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken) =>
        store.GetPostAsync(slug, now, cancellationToken);
}
