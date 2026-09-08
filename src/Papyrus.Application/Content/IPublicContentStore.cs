using Papyrus.Contracts.Content;

namespace Papyrus.Application.Content;

/// <summary>
/// Application-owned port for reading public (published, live) content. The
/// application layer depends on this interface rather than on a persistence
/// implementation, keeping EF/PostgreSQL concerns behind an adapter.
/// </summary>
public interface IPublicContentStore
{
    Task<PublicBlogIndexResult> GetBlogIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken);

    Task<PublicPostPageResult?> GetPostPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken);

    Task<PublicPortfolioIndexResult> GetPortfolioIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken);

    Task<PublicProjectPageResult?> GetProjectPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken);

    Task<PublicProjectDetail?> GetProjectAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken);

    Task<PublicPostDetail?> GetPostAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken);
}
