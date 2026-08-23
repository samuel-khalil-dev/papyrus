using Microsoft.EntityFrameworkCore;
using Papyrus.Contracts.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Application.Content;

public sealed class PublicContentQueryHandlers(PapyrusDbContext db)
{
    public async Task<PublicBlogIndexResult> GetBlogIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken)
    {
        var published = db.BlogPosts.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().Length > 128 ? searchTerm.Trim()[..128] : searchTerm.Trim();
            published = IsInMemory()
                ? published.Where(x => x.Title.ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || x.Summary.ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || x.ContentMarkdown.ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || x.BlogPostTags.Any(link => link.Tag.Name.ToLowerInvariant().Contains(term.ToLowerInvariant()) || link.Tag.Slug.ToLowerInvariant().Contains(term.ToLowerInvariant())))
                : published.Where(x => EF.Functions.ILike(x.Title, $"%{term}%")
                    || EF.Functions.ILike(x.Summary, $"%{term}%")
                    || EF.Functions.ILike(x.ContentMarkdown, $"%{term}%")
                    || x.BlogPostTags.Any(link => EF.Functions.ILike(link.Tag.Name, $"%{term}%") || EF.Functions.ILike(link.Tag.Slug, $"%{term}%")));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim();
            published = IsInMemory()
                ? published.Where(x => x.BlogPostTags.Any(link => link.Tag.Slug.ToLowerInvariant() == normalizedTag.ToLowerInvariant() || link.Tag.Name.ToLowerInvariant() == normalizedTag.ToLowerInvariant()))
                : published.Where(x => x.BlogPostTags.Any(link => EF.Functions.ILike(link.Tag.Slug, normalizedTag) || EF.Functions.ILike(link.Tag.Name, normalizedTag)));
        }

        var totalPosts = await published.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalPosts / (double)pageSize));
        var currentPage = Math.Min(Math.Max(1, page), totalPages);
        var posts = await published.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(PostCardProjection())
            .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var featured = await db.BlogPosts.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now))
            .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(PostCardProjection()).FirstOrDefaultAsync(cancellationToken);
        var tags = await GetBlogTagsAsync(cancellationToken);
        return new(featured, posts, tags, totalPosts, totalPages);
    }

    public async Task<PublicPostPageResult?> GetPostPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var post = await GetPostAsync(slug, now, cancellationToken);
        if (post is null) return null;

        var related = await db.BlogPosts.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now) && x.Slug != slug
                && ((post.Category != null && x.Category != null && x.Category.Slug == post.Category.Slug)
                    || (post.Project != null && x.Project != null && x.Project.Slug == post.Project.Slug)))
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).Take(3)
            .Select(PostCardProjection()).ToListAsync(cancellationToken);
        return new(post, related, await GetBlogTagsAsync(cancellationToken));
    }

    public async Task<PublicPortfolioIndexResult> GetPortfolioIndexAsync(
        string? searchTerm, string? tag, DateTimeOffset now, int page, int pageSize, CancellationToken cancellationToken)
    {
        var published = db.Projects.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().Length > 128 ? searchTerm.Trim()[..128] : searchTerm.Trim();
            published = IsInMemory()
                ? published.Where(x => x.Title.ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || x.Summary.ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || (x.Description ?? string.Empty).ToLowerInvariant().Contains(term.ToLowerInvariant())
                    || x.ProjectTags.Any(link => link.Tag.Name.ToLowerInvariant().Contains(term.ToLowerInvariant()) || link.Tag.Slug.ToLowerInvariant().Contains(term.ToLowerInvariant())))
                : published.Where(x => EF.Functions.ILike(x.Title, $"%{term}%")
                    || EF.Functions.ILike(x.Summary, $"%{term}%")
                    || EF.Functions.ILike(x.Description ?? string.Empty, $"%{term}%")
                    || x.ProjectTags.Any(link => EF.Functions.ILike(link.Tag.Name, $"%{term}%") || EF.Functions.ILike(link.Tag.Slug, $"%{term}%")));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim();
            published = IsInMemory()
                ? published.Where(x => x.ProjectTags.Any(link => link.Tag.Slug.ToLowerInvariant() == normalizedTag.ToLowerInvariant() || link.Tag.Name.ToLowerInvariant() == normalizedTag.ToLowerInvariant()))
                : published.Where(x => x.ProjectTags.Any(link => EF.Functions.ILike(link.Tag.Slug, normalizedTag) || EF.Functions.ILike(link.Tag.Name, normalizedTag)));
        }

        var totalProjects = await published.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalProjects / (double)pageSize));
        var currentPage = Math.Min(Math.Max(1, page), totalPages);
        var projects = await published.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(ProjectCardProjection()).Skip((currentPage - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var featured = await db.Projects.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now))
            .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(ProjectCardProjection()).FirstOrDefaultAsync(cancellationToken);
        var tags = await GetProjectTagsAsync(cancellationToken);
        return new(featured, projects, tags, totalProjects, totalPages);
    }

    public async Task<PublicProjectPageResult?> GetProjectPageAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var project = await GetProjectAsync(slug, now, cancellationToken);
        if (project is null) return null;

        var relatedProjects = await db.Projects.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now) && x.Slug != slug
                && project.Category != null && x.Category != null && x.Category.Slug == project.Category.Slug)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).Take(3)
            .Select(ProjectCardProjection()).ToListAsync(cancellationToken);
        var relatedPosts = await db.BlogPosts.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now)
                && ((x.Project != null && x.Project.Slug == slug)
                    || (project.Category != null && x.Category != null && x.Category.Slug == project.Category.Slug)))
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).Take(4)
            .Select(PostCardProjection()).ToListAsync(cancellationToken);
        return new(project, relatedProjects, relatedPosts, await GetProjectTagsAsync(cancellationToken));
    }

    private static System.Linq.Expressions.Expression<Func<BlogPost, PublicPostCard>> PostCardProjection() =>
        x => new PublicPostCard(x.Id, x.Slug, x.Title, x.Summary, x.CoverImageUrl, x.IsFeatured, x.PublishedAt, x.CreatedAt,
            x.Category == null ? null : new PublicCategory(x.Category.Slug, x.Category.Name),
            x.Project == null ? null : new PublicProjectCard(x.Project.Id, x.Project.Slug, x.Project.Title, x.Project.Summary,
                x.Project.CoverImageUrl, x.Project.IsFeatured, x.Project.PublishedAt, x.Project.CreatedAt, null, Array.Empty<PublicTag>()),
            x.BlogPostTags.OrderBy(link => link.Tag.Name).Select(link => new PublicTag(link.Tag.Slug, link.Tag.Name)).ToArray());

    private static System.Linq.Expressions.Expression<Func<Project, PublicProjectCard>> ProjectCardProjection() =>
        x => new PublicProjectCard(x.Id, x.Slug, x.Title, x.Summary, x.CoverImageUrl, x.IsFeatured, x.PublishedAt, x.CreatedAt,
            x.Category == null ? null : new PublicCategory(x.Category.Slug, x.Category.Name),
            x.ProjectTags.OrderBy(link => link.Tag.Name).Select(link => new PublicTag(link.Tag.Slug, link.Tag.Name)).ToArray());

    public async Task<PublicProjectDetail?> GetProjectAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var project = await db.Projects.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now) && x.Slug == slug)
            .Select(x => new PublicProjectDetail(
                x.Id, x.Slug, x.Title, x.Summary, x.Description, x.RepositoryUrl, x.LiveUrl, x.CoverImageUrl,
                x.IsFeatured, x.PublishedAt, x.CreatedAt,
                x.Category == null ? null : new PublicCategory(x.Category.Slug, x.Category.Name),
                x.ProjectTags.OrderBy(link => link.Tag.Name).Select(link => new PublicTag(link.Tag.Slug, link.Tag.Name)).ToArray(),
                Array.Empty<PublicPostCard>()))
            .FirstOrDefaultAsync(cancellationToken);
        return project;
    }

    public Task<PublicPostDetail?> GetPostAsync(string slug, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.BlogPosts.AsNoTracking()
            .Where(x => x.IsPublished && (x.PublishedAt == null || x.PublishedAt <= now) && x.Slug == slug)
            .Select(x => new PublicPostDetail(
                x.Id, x.Slug, x.Title, x.Summary, x.ContentMarkdown, x.CoverImageUrl, x.IsFeatured,
                x.PublishedAt, x.CreatedAt,
                x.Category == null ? null : new PublicCategory(x.Category.Slug, x.Category.Name),
                x.Project == null ? null : new PublicProjectCard(
                    x.Project.Id, x.Project.Slug, x.Project.Title, x.Project.Summary, x.Project.CoverImageUrl,
                    x.Project.IsFeatured, x.Project.PublishedAt, x.Project.CreatedAt, null, Array.Empty<PublicTag>()),
                x.BlogPostTags.OrderBy(link => link.Tag.Name).Select(link => new PublicTag(link.Tag.Slug, link.Tag.Name)).ToArray()))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<PublicTag>> GetBlogTagsAsync(CancellationToken cancellationToken) =>
        db.BlogPostTags.AsNoTracking()
            .Where(x => x.BlogPost.IsPublished)
            .Select(x => new { x.Tag.Slug, x.Tag.Name })
            .Distinct().OrderBy(x => x.Name).Take(20)
            .Select(x => new PublicTag(x.Slug, x.Name)).ToListAsync(cancellationToken);

    public Task<List<PublicTag>> GetProjectTagsAsync(CancellationToken cancellationToken) =>
        db.ProjectTags.AsNoTracking()
            .Where(x => x.Project.IsPublished)
            .Select(x => new { x.Tag.Slug, x.Tag.Name })
            .Distinct().OrderBy(x => x.Name).Take(20)
            .Select(x => new PublicTag(x.Slug, x.Name)).ToListAsync(cancellationToken);

    private bool IsInMemory() => string.Equals(db.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal);
}
