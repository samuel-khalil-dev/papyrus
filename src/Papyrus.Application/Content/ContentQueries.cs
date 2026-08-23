using Microsoft.EntityFrameworkCore;
using Papyrus.Infrastructure;

namespace Papyrus.Application.Content;

public sealed record ContentListItem(Guid Id, string Slug, string Title, DateTimeOffset? PublishedAt, DateTimeOffset CreatedAt);

public sealed class ContentQueryHandlers(PapyrusDbContext db)
{
    public Task<List<ContentListItem>> GetProjectsAsync(CancellationToken cancellationToken) =>
        db.Projects.AsNoTracking().OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(x => new ContentListItem(x.Id, x.Slug, x.Title, x.PublishedAt, x.CreatedAt)).ToListAsync(cancellationToken);

    public Task<List<ContentListItem>> GetBlogPostsAsync(CancellationToken cancellationToken) =>
        db.BlogPosts.AsNoTracking().OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(x => new ContentListItem(x.Id, x.Slug, x.Title, x.PublishedAt, x.CreatedAt)).ToListAsync(cancellationToken);
}
