using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Api.Admin.AdminUi;

internal static class AdminContentSupport
{
    public static IReadOnlyList<string> ParseTagNames(string? tagsCsv)
    {
        if (string.IsNullOrWhiteSpace(tagsCsv))
        {
            return [];
        }

        return tagsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string Slugify(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(slug))
        {
            return "item";
        }

        return slug.Length <= 200 ? slug : slug[..200].Trim('-');
    }

    public static async Task<IReadOnlyList<ContentTag>> ResolveTagsAsync(PapyrusDbContext db, string? tagsCsv)
    {
        var names = ParseTagNames(tagsCsv);
        if (names.Count == 0)
        {
            return [];
        }

        var tags = new List<ContentTag>(names.Count);
        foreach (var name in names)
        {
            var slug = Slugify(name);
            var existing = await db.ContentTags.FirstOrDefaultAsync(tag =>
                tag.Slug == slug || tag.Name.ToLower() == name.ToLower());

            if (existing is not null)
            {
                tags.Add(existing);
                continue;
            }

            var tag = new ContentTag
            {
                Id = Guid.NewGuid(),
                TenantId = db.TenantId,
                Slug = slug,
                Name = name,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.ContentTags.Add(tag);
            tags.Add(tag);
        }

        return tags;
    }

    public static async Task SetBlogPostTagsAsync(PapyrusDbContext db, Guid blogPostId, IReadOnlyCollection<ContentTag> tags)
    {
        var current = await db.BlogPostTags.Where(blogPostTag => blogPostTag.BlogPostId == blogPostId).ToListAsync();
        db.BlogPostTags.RemoveRange(current);

        foreach (var tag in tags)
        {
            db.BlogPostTags.Add(new BlogPostTag { BlogPostId = blogPostId, TagId = tag.Id, TenantId = db.TenantId });
        }
    }

    public static async Task SetProjectTagsAsync(PapyrusDbContext db, Guid projectId, IReadOnlyCollection<ContentTag> tags)
    {
        var current = await db.ProjectTags.Where(projectTag => projectTag.ProjectId == projectId).ToListAsync();
        db.ProjectTags.RemoveRange(current);

        foreach (var tag in tags)
        {
            db.ProjectTags.Add(new ProjectTag { ProjectId = projectId, TagId = tag.Id, TenantId = db.TenantId });
        }
    }

    public static async Task<bool> IsMediaReferencedAsync(PapyrusDbContext db, string fileName, CancellationToken cancellationToken = default)
    {
        var mediaPath = $"/media/{db.TenantId}/{fileName}";
        var mediaSuffix = $"/{fileName}";

        return await db.Projects.AsNoTracking().AnyAsync(project =>
                   project.CoverImageUrl == mediaPath
                   || (project.CoverImageUrl != null && project.CoverImageUrl.EndsWith(mediaSuffix)), cancellationToken)
            || await db.BlogPosts.AsNoTracking().AnyAsync(post =>
                   post.CoverImageUrl == mediaPath
                   || (post.CoverImageUrl != null && post.CoverImageUrl.EndsWith(mediaSuffix)), cancellationToken);
    }
}
