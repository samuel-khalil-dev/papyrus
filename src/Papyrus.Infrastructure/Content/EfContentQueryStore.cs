using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// EF Core adapter for <see cref="IContentQueryStore"/>.
/// </summary>
public sealed class EfContentQueryStore(PapyrusDbContext db) : IContentQueryStore
{
    public Task<List<ContentListItem>> GetProjectsAsync(CancellationToken cancellationToken) =>
        db.Projects.AsNoTracking().OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(x => new ContentListItem(x.Id, x.Slug, x.Title, x.PublishedAt, x.CreatedAt)).ToListAsync(cancellationToken);

    public Task<List<ContentListItem>> GetBlogPostsAsync(CancellationToken cancellationToken) =>
        db.BlogPosts.AsNoTracking().OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(x => new ContentListItem(x.Id, x.Slug, x.Title, x.PublishedAt, x.CreatedAt)).ToListAsync(cancellationToken);
}
