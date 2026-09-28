using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Api.Admin.Endpoints;

internal static class AdminEndpointSupport
{
    public static async Task<Project?> LoadProjectAsync(PapyrusDbContext db, Guid projectId, CancellationToken cancellationToken) =>
        await db.Projects.AsNoTracking()
            .Include(project => project.Category)
            .Include(project => project.ProjectTags).ThenInclude(projectTag => projectTag.Tag)
            .Include(project => project.BlogPosts)
            .FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

    public static async Task<BlogPost?> LoadBlogPostAsync(PapyrusDbContext db, Guid blogPostId, CancellationToken cancellationToken) =>
        await db.BlogPosts.AsNoTracking()
            .Include(post => post.Category)
            .Include(post => post.Project)
            .Include(post => post.BlogPostTags).ThenInclude(blogPostTag => blogPostTag.Tag)
            .FirstOrDefaultAsync(post => post.Id == blogPostId, cancellationToken);

    public static System.Linq.Expressions.Expression<Func<ContentCategory, ContentCategoryResponse>> CategoryProjection() =>
        category => new ContentCategoryResponse(category.Id, category.Slug, category.Name, category.Description,
            category.IsPublished, category.CreatedAt, category.UpdatedAt);

    public static System.Linq.Expressions.Expression<Func<ContentTag, ContentTagResponse>> TagProjection() =>
        tag => new ContentTagResponse(tag.Id, tag.Slug, tag.Name, tag.CreatedAt, tag.UpdatedAt);

    public static System.Linq.Expressions.Expression<Func<Project, ProjectResponse>> ProjectProjection() =>
        project => new ProjectResponse(
            project.Id, project.Slug, project.Title, project.Summary, project.Description,
            project.RepositoryUrl, project.LiveUrl, project.CoverImageUrl, project.IsFeatured,
            project.IsPublished, project.PublishedAt, project.CreatedAt, project.UpdatedAt,
            project.Category == null ? null : new ContentCategoryResponse(project.Category.Id, project.Category.Slug,
                project.Category.Name, project.Category.Description, project.Category.IsPublished,
                project.Category.CreatedAt, project.Category.UpdatedAt),
            project.ProjectTags.OrderBy(link => link.Tag.Name)
                .Take(50).Select(link => new ContentTagResponse(link.Tag.Id, link.Tag.Slug, link.Tag.Name,
                    link.Tag.CreatedAt, link.Tag.UpdatedAt)).ToArray(),
            project.BlogPosts.OrderBy(post => post.PublishedAt ?? post.CreatedAt)
                .Take(50).Select(post => new BlogPostSummaryResponse(post.Id, post.Slug, post.Title, post.Summary,
                    post.IsFeatured, post.IsPublished, post.PublishedAt)).ToArray());

    public static System.Linq.Expressions.Expression<Func<BlogPost, BlogPostResponse>> BlogPostProjection() =>
        post => new BlogPostResponse(
            post.Id, post.Slug, post.Title, post.Summary, post.ContentMarkdown, post.CoverImageUrl,
            post.IsFeatured, post.IsPublished, post.PublishedAt, post.CreatedAt, post.UpdatedAt,
            post.Category == null ? null : new ContentCategoryResponse(post.Category.Id, post.Category.Slug,
                post.Category.Name, post.Category.Description, post.Category.IsPublished,
                post.Category.CreatedAt, post.Category.UpdatedAt),
            post.Project == null ? null : new ProjectSummaryResponse(post.Project.Id, post.Project.Slug,
                post.Project.Title, post.Project.Summary, post.Project.CoverImageUrl, post.Project.IsFeatured,
                post.Project.IsPublished, post.Project.PublishedAt),
            post.BlogPostTags.OrderBy(link => link.Tag.Name)
                .Take(50).Select(link => new ContentTagResponse(link.Tag.Id, link.Tag.Slug, link.Tag.Name,
                    link.Tag.CreatedAt, link.Tag.UpdatedAt)).ToArray());

    public static async Task<IResult> PaginateQueryAsync<T>(IQueryable<T> query, int? requestedPage,
        int? requestedPageSize, bool versioned, CancellationToken cancellationToken = default)
    {
        if (!versioned)
        {
            var compatibilityPageSize = Math.Clamp(requestedPageSize ?? 20, 1, 100);
            var compatibilityTotalItems = await query.CountAsync(cancellationToken);
            var compatibilityTotalPages = Math.Max(1, (int)Math.Ceiling(compatibilityTotalItems / (double)compatibilityPageSize));
            var compatibilityPage = Math.Clamp(requestedPage ?? 1, 1, compatibilityTotalPages);
            var compatibilityPageItems = await query.Skip((compatibilityPage - 1) * compatibilityPageSize)
                .Take(compatibilityPageSize).ToArrayAsync(cancellationToken);
            return Results.Ok(compatibilityPageItems);
        }

        var pageSize = Math.Clamp(requestedPageSize ?? 20, 1, 100);
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        var page = Math.Clamp(requestedPage ?? 1, 1, totalPages);
        var pageItems = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return Results.Ok(new PageResponse<T>(pageItems, page, pageSize, totalItems, totalPages));
    }

    public static IResult Paginate<T>(IReadOnlyList<T> items, int? requestedPage, int? requestedPageSize, bool versioned)
    {
        if (!versioned)
        {
            return Results.Ok(items);
        }

        var pageSize = Math.Clamp(requestedPageSize ?? 20, 1, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(items.Count / (double)pageSize));
        var page = Math.Clamp(requestedPage ?? 1, 1, totalPages);
        var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Results.Ok(new PageResponse<T>(pageItems, page, pageSize, items.Count, totalPages));
    }

    public static void WithCollectionResponse<T>(RouteHandlerBuilder endpoint, bool versioned)
    {
        if (versioned)
        {
            endpoint.Produces<PageResponse<T>>();
        }
        else
        {
            endpoint.Produces<IReadOnlyList<T>>();
        }
    }

    public static string ResourcePath(string collection, Guid id, bool versioned) =>
        $"/{(versioned ? "api/v1" : "api")}/{collection}/{id}";

    public static IResult ValidationProblem(IReadOnlyList<ValidationError> errors) =>
        Results.ValidationProblem(
            errors.GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray()),
            title: "The request contains validation errors.",
            detail: "Correct the indicated fields and try again.");

    /// <summary>
    /// Normalises an optional tag-id list to a non-null collection.
    /// <para>
    /// <c>tagIds</c> has no <c>[Required]</c> attribute, so a request that omits
    /// the property binds it to <c>null</c> even though the contract declares a
    /// non-nullable array. Passing that <c>null</c> into the command store
    /// produced an unhandled exception and a 500 for a perfectly reasonable
    /// request that simply asked for no tags. An omitted collection means the
    /// same thing as an empty one here.
    /// </para>
    /// </summary>
    public static Guid[] Tags(Guid[]? tagIds) => tagIds ?? [];

    /// <summary>
    /// Maps a failed command result onto the right status: 409 for a slug
    /// conflict, 400 for everything else.
    /// <para>
    /// A slug conflict is deliberately not a <c>ValidationProblemDetails</c>:
    /// the request is well formed and a different resource holds the slug, so
    /// the response names that resource instead of listing field errors.
    /// </para>
    /// </summary>
    public static IResult CommandFailure(IReadOnlyList<ValidationError> errors) =>
        ContentValidation.HasSlugConflict(errors)
            ? Results.Problem(
                title: "The slug is already in use.",
                detail: errors.First(ContentValidation.IsSlugConflict).Message,
                statusCode: StatusCodes.Status409Conflict)
            : ValidationProblem(errors);

    public static ContentCategoryResponse ToResponse(this ContentCategory category) =>
        new(category.Id, category.Slug, category.Name, category.Description, category.IsPublished, category.CreatedAt, category.UpdatedAt);

    public static ContentTagResponse ToResponse(this ContentTag tag) =>
        new(tag.Id, tag.Slug, tag.Name, tag.CreatedAt, tag.UpdatedAt);

    public static ProjectSummaryResponse ToSummary(this Project project) =>
        new(project.Id, project.Slug, project.Title, project.Summary, project.CoverImageUrl, project.IsFeatured, project.IsPublished, project.PublishedAt);

    public static BlogPostSummaryResponse ToSummary(this BlogPost post) =>
        new(post.Id, post.Slug, post.Title, post.Summary, post.IsFeatured, post.IsPublished, post.PublishedAt);

    public static ProjectResponse ToResponse(this Project project) =>
        new(
            project.Id,
            project.Slug,
            project.Title,
            project.Summary,
            project.Description,
            project.RepositoryUrl,
            project.LiveUrl,
            project.CoverImageUrl,
            project.IsFeatured,
            project.IsPublished,
            project.PublishedAt,
            project.CreatedAt,
            project.UpdatedAt,
            project.Category?.ToResponse(),
            project.ProjectTags.Select(projectTag => projectTag.Tag.ToResponse()).ToArray(),
            project.BlogPosts.Select(blogPost => blogPost.ToSummary()).ToArray());

    public static BlogPostResponse ToResponse(this BlogPost blogPost) =>
        new(
            blogPost.Id,
            blogPost.Slug,
            blogPost.Title,
            blogPost.Summary,
            blogPost.ContentMarkdown,
            blogPost.CoverImageUrl,
            blogPost.IsFeatured,
            blogPost.IsPublished,
            blogPost.PublishedAt,
            blogPost.CreatedAt,
            blogPost.UpdatedAt,
            blogPost.Category?.ToResponse(),
            blogPost.Project?.ToSummary(),
            blogPost.BlogPostTags.Select(blogPostTag => blogPostTag.Tag.ToResponse()).ToArray());
}
