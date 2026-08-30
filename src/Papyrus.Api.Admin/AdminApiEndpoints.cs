using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Papyrus.Api.Admin.AdminUi;
using Papyrus.Contracts.Content;
using Papyrus.Api.Admin.Media;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Application.Content;

namespace Papyrus.Api.Admin;

internal static class AdminApiEndpoints
{
    public static IEndpointRouteBuilder MapAdminApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").RequireAuthorization().RequireRateLimiting("admin-api");

        MapCategoryEndpoints(api, true);
        MapTagEndpoints(api, true);
        MapProjectEndpoints(api, true);
        MapBlogPostEndpoints(api, true);
        MapMediaEndpoints(api);

        // Keep the original unversioned surface during the compatibility window.
        var legacyApi = app.MapGroup("/api").RequireAuthorization().RequireRateLimiting("admin-api").ExcludeFromDescription();
        MapCategoryEndpoints(legacyApi, false);
        MapTagEndpoints(legacyApi, false);
        MapProjectEndpoints(legacyApi, false);
        MapBlogPostEndpoints(legacyApi, false);
        MapMediaEndpoints(legacyApi);

        return app;
    }

    private static void MapCategoryEndpoints(RouteGroupBuilder api, bool versioned)
    {
        WithCollectionResponse<ContentCategoryResponse>(api.MapGet("/categories", async (int? page, int? pageSize, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var query = db.ContentCategories.AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(CategoryProjection());
            return await PaginateQueryAsync(query, page, pageSize, versioned, cancellationToken);
        }), versioned);

        api.MapGet("/categories/{id:guid}", async (Guid id, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var category = await db.ContentCategories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
            return category is null ? Results.NotFound() : Results.Ok(category.ToResponse());
        }).Produces<ContentCategoryResponse>().Produces(StatusCodes.Status404NotFound);

        api.MapPost("/categories", async (CategoryUpsertRequest request, [FromServices] TaxonomyCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveCategoryAsync(new(null, request.Slug, request.Name, request.Description, request.IsPublished), cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/categories/{result.Value!.Id}", result.Value.ToResponse())
                : ValidationProblem(result.Errors);
        }).Produces<ContentCategoryResponse>(StatusCodes.Status201Created).Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapPut("/categories/{id:guid}", async (Guid id, CategoryUpsertRequest request, [FromServices] TaxonomyCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveCategoryAsync(new(id, request.Slug, request.Name, request.Description, request.IsPublished), cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value!.ToResponse())
                : result.Errors.Any(error => error.Code == "category.not_found") ? Results.NotFound() : ValidationProblem(result.Errors);
        }).Produces<ContentCategoryResponse>().Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound);

        api.MapDelete("/categories/{id:guid}", async (Guid id, [FromServices] ContentLifecycleHandlers handlers, CancellationToken cancellationToken) =>
        {
            return await handlers.SoftDeleteCategoryAsync(id, "Deleted through the admin API.", cancellationToken)
                ? Results.NoContent() : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    private static void MapTagEndpoints(RouteGroupBuilder api, bool versioned)
    {
        WithCollectionResponse<ContentTagResponse>(api.MapGet("/tags", async (int? page, int? pageSize, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var query = db.ContentTags.AsNoTracking()
                .OrderBy(tag => tag.Name)
                .Select(TagProjection());
            return await PaginateQueryAsync(query, page, pageSize, versioned, cancellationToken);
        }), versioned);

        api.MapGet("/tags/{id:guid}", async (Guid id, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var tag = await db.ContentTags.AsNoTracking().FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);
            return tag is null ? Results.NotFound() : Results.Ok(tag.ToResponse());
        }).Produces<ContentTagResponse>().Produces(StatusCodes.Status404NotFound);

        api.MapPost("/tags", async (TagUpsertRequest request, [FromServices] TaxonomyCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveTagAsync(new(null, request.Slug, request.Name), cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/tags/{result.Value!.Id}", result.Value.ToResponse())
                : ValidationProblem(result.Errors);
        }).Produces<ContentTagResponse>(StatusCodes.Status201Created).Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapPut("/tags/{id:guid}", async (Guid id, TagUpsertRequest request, [FromServices] TaxonomyCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveTagAsync(new(id, request.Slug, request.Name), cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value!.ToResponse())
                : result.Errors.Any(error => error.Code == "tag.not_found") ? Results.NotFound() : ValidationProblem(result.Errors);
        }).Produces<ContentTagResponse>().Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound);

        api.MapDelete("/tags/{id:guid}", async (Guid id, [FromServices] ContentLifecycleHandlers handlers, CancellationToken cancellationToken) =>
        {
            return await handlers.SoftDeleteTagAsync(id, "Deleted through the admin API.", cancellationToken)
                ? Results.NoContent() : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    private static void MapProjectEndpoints(RouteGroupBuilder api, bool versioned)
    {
        WithCollectionResponse<ProjectResponse>(api.MapGet("/projects", async (int? page, int? pageSize, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var query = db.Projects.AsNoTracking()
                .OrderByDescending(project => project.IsFeatured)
                .ThenByDescending(project => project.PublishedAt ?? project.CreatedAt)
                .Select(ProjectProjection());
            return await PaginateQueryAsync(query, page, pageSize, versioned, cancellationToken);
        }), versioned);

        api.MapGet("/projects/{id:guid}", async (Guid id, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var project = await db.Projects.AsNoTracking()
                .Select(ProjectProjection())
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            return project is null ? Results.NotFound() : Results.Ok(project);
        }).Produces<ProjectResponse>().Produces(StatusCodes.Status404NotFound);

        api.MapPost("/projects", async (ProjectUpsertRequest request, PapyrusDbContext db, [FromServices] ContentCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveProjectAsync(new(null, request.Slug, request.Title, request.Summary, request.Description,
                request.RepositoryUrl, request.LiveUrl, request.CoverImageUrl, request.CategoryId, request.IsFeatured,
                request.IsPublished, request.PublishedAt, request.TagIds), cancellationToken);
            if (!result.IsSuccess) return ValidationProblem(result.Errors);
            var project = result.Value!;
            var created = await LoadProjectAsync(db, project.Id, cancellationToken);
            return Results.Created(ResourcePath("projects", project.Id, versioned), created!.ToResponse());
        }).Produces<ProjectResponse>(StatusCodes.Status201Created).Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapPut("/projects/{id:guid}", async (Guid id, ProjectUpsertRequest request, PapyrusDbContext db, [FromServices] ContentCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveProjectAsync(new(id, request.Slug, request.Title, request.Summary, request.Description,
                request.RepositoryUrl, request.LiveUrl, request.CoverImageUrl, request.CategoryId, request.IsFeatured,
                request.IsPublished, request.PublishedAt, request.TagIds), cancellationToken);
            if (!result.IsSuccess) return result.Errors.Any(x => x.Code == "project.not_found") ? Results.NotFound() : ValidationProblem(result.Errors);
            var updated = await LoadProjectAsync(db, id, cancellationToken);
            return Results.Ok(updated!.ToResponse());
        }).Produces<ProjectResponse>().Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound);

        api.MapDelete("/projects/{id:guid}", async (Guid id, [FromServices] ContentLifecycleHandlers handlers, CancellationToken cancellationToken) =>
        {
            return await handlers.SoftDeleteProjectAsync(id, "Deleted through the admin API.", cancellationToken)
                ? Results.NoContent() : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    private static void MapBlogPostEndpoints(RouteGroupBuilder api, bool versioned)
    {
        WithCollectionResponse<BlogPostResponse>(api.MapGet("/blog-posts", async (int? page, int? pageSize, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var query = db.BlogPosts.AsNoTracking()
                .OrderByDescending(post => post.PublishedAt ?? post.CreatedAt)
                .Select(BlogPostProjection());
            return await PaginateQueryAsync(query, page, pageSize, versioned, cancellationToken);
        }), versioned);

        api.MapGet("/blog-posts/{id:guid}", async (Guid id, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            var blogPost = await db.BlogPosts.AsNoTracking()
                .Select(BlogPostProjection())
                .FirstOrDefaultAsync(post => post.Id == id, cancellationToken);

            return blogPost is null ? Results.NotFound() : Results.Ok(blogPost);
        }).Produces<BlogPostResponse>().Produces(StatusCodes.Status404NotFound);

        api.MapPost("/blog-posts", async (BlogPostUpsertRequest request, PapyrusDbContext db, [FromServices] ContentCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveBlogPostAsync(new(null, request.Slug, request.Title, request.Summary, request.ContentMarkdown,
                request.CoverImageUrl, request.ProjectId, request.CategoryId, request.IsFeatured, request.IsPublished,
                request.PublishedAt, request.TagIds), cancellationToken);
            if (!result.IsSuccess) return ValidationProblem(result.Errors);
            var blogPost = result.Value!;
            var created = await LoadBlogPostAsync(db, blogPost.Id, cancellationToken);
            return Results.Created(ResourcePath("blog-posts", blogPost.Id, versioned), created!.ToResponse());
        }).Produces<BlogPostResponse>(StatusCodes.Status201Created).Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapPut("/blog-posts/{id:guid}", async (Guid id, BlogPostUpsertRequest request, PapyrusDbContext db, [FromServices] ContentCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveBlogPostAsync(new(id, request.Slug, request.Title, request.Summary, request.ContentMarkdown,
                request.CoverImageUrl, request.ProjectId, request.CategoryId, request.IsFeatured, request.IsPublished,
                request.PublishedAt, request.TagIds), cancellationToken);
            if (!result.IsSuccess) return result.Errors.Any(x => x.Code == "post.not_found") ? Results.NotFound() : ValidationProblem(result.Errors);
            var updated = await LoadBlogPostAsync(db, id, cancellationToken);
            return Results.Ok(updated!.ToResponse());
        }).Produces<BlogPostResponse>().Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound);

        api.MapDelete("/blog-posts/{id:guid}", async (Guid id, [FromServices] ContentLifecycleHandlers handlers, CancellationToken cancellationToken) =>
        {
            return await handlers.SoftDeleteBlogPostAsync(id, "Deleted through the admin API.", cancellationToken)
                ? Results.NoContent() : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    private static void MapMediaEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/media", async (LocalMediaService media, CancellationToken cancellationToken) =>
            Results.Ok(await media.ListAsync(cancellationToken)))
            .Produces<IReadOnlyList<MediaFileResult>>();

        api.MapPost("/media", async (HttpRequest request, LocalMediaService media, CancellationToken cancellationToken) =>
        {
            var file = request.Form.Files["file"] ?? request.Form.Files.FirstOrDefault();
            if (file is null)
            {
                return Results.BadRequest("A file named 'file' is required.");
            }

            try
            {
                var uploaded = await media.SaveAsync(file, cancellationToken);
                return Results.Ok(uploaded);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(exception.Message);
            }
        }).Produces<MediaUploadResult>().Produces<string>(StatusCodes.Status400BadRequest)
            .Accepts<IFormFile>("multipart/form-data").DisableAntiforgery();

        api.MapDelete("/media/{fileName}", async (string fileName, LocalMediaService media, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            if (await AdminContentSupport.IsMediaReferencedAsync(db, fileName, cancellationToken))
            {
                return Results.Conflict("The media file is still referenced by published content or an admin draft.");
            }

            return await media.DeleteAsync(fileName, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).Produces<string>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Project?> LoadProjectAsync(PapyrusDbContext db, Guid projectId, CancellationToken cancellationToken) =>
        await db.Projects.AsNoTracking()
            .Include(project => project.Category)
            .Include(project => project.ProjectTags).ThenInclude(projectTag => projectTag.Tag)
            .Include(project => project.BlogPosts)
            .FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

    private static async Task<BlogPost?> LoadBlogPostAsync(PapyrusDbContext db, Guid blogPostId, CancellationToken cancellationToken) =>
        await db.BlogPosts.AsNoTracking()
            .Include(post => post.Category)
            .Include(post => post.Project)
            .Include(post => post.BlogPostTags).ThenInclude(blogPostTag => blogPostTag.Tag)
            .FirstOrDefaultAsync(post => post.Id == blogPostId, cancellationToken);

    private static System.Linq.Expressions.Expression<Func<ContentCategory, ContentCategoryResponse>> CategoryProjection() =>
        category => new ContentCategoryResponse(category.Id, category.Slug, category.Name, category.Description,
            category.IsPublished, category.CreatedAt, category.UpdatedAt);

    private static System.Linq.Expressions.Expression<Func<ContentTag, ContentTagResponse>> TagProjection() =>
        tag => new ContentTagResponse(tag.Id, tag.Slug, tag.Name, tag.CreatedAt, tag.UpdatedAt);

    private static System.Linq.Expressions.Expression<Func<Project, ProjectResponse>> ProjectProjection() =>
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

    private static System.Linq.Expressions.Expression<Func<BlogPost, BlogPostResponse>> BlogPostProjection() =>
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

    private static async Task<IResult> PaginateQueryAsync<T>(IQueryable<T> query, int? requestedPage,
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

    private static IResult Paginate<T>(IReadOnlyList<T> items, int? requestedPage, int? requestedPageSize, bool versioned)
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

    private static void WithCollectionResponse<T>(RouteHandlerBuilder endpoint, bool versioned)
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

    private static string ResourcePath(string collection, Guid id, bool versioned) =>
        $"/{(versioned ? "api/v1" : "api")}/{collection}/{id}";

    private static IResult ValidationProblem(IReadOnlyList<ValidationError> errors) =>
        Results.ValidationProblem(
            errors.GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray()),
            title: "The request contains validation errors.",
            detail: "Correct the indicated fields and try again.");

    private static ContentCategoryResponse ToResponse(this ContentCategory category) =>
        new(category.Id, category.Slug, category.Name, category.Description, category.IsPublished, category.CreatedAt, category.UpdatedAt);

    private static ContentTagResponse ToResponse(this ContentTag tag) =>
        new(tag.Id, tag.Slug, tag.Name, tag.CreatedAt, tag.UpdatedAt);

    private static ProjectSummaryResponse ToSummary(this Project project) =>
        new(project.Id, project.Slug, project.Title, project.Summary, project.CoverImageUrl, project.IsFeatured, project.IsPublished, project.PublishedAt);

    private static BlogPostSummaryResponse ToSummary(this BlogPost post) =>
        new(post.Id, post.Slug, post.Title, post.Summary, post.IsFeatured, post.IsPublished, post.PublishedAt);

    private static ProjectResponse ToResponse(this Project project) =>
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

    private static BlogPostResponse ToResponse(this BlogPost blogPost) =>
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
