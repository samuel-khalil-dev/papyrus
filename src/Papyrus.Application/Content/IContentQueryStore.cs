namespace Papyrus.Application.Content;

/// <summary>
/// Application-owned port for admin content listing queries.
/// </summary>
public interface IContentQueryStore
{
    Task<List<ContentListItem>> GetProjectsAsync(CancellationToken cancellationToken);

    Task<List<ContentListItem>> GetBlogPostsAsync(CancellationToken cancellationToken);
}
