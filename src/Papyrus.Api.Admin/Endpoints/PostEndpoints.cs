using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Infrastructure;
using static Papyrus.Api.Admin.Endpoints.AdminEndpointSupport;

namespace Papyrus.Api.Admin.Endpoints;

internal static class PostEndpoints
{
    public static void MapPostEndpoints(this RouteGroupBuilder api, bool versioned)
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
                request.PublishedAt, Tags(request.TagIds)), cancellationToken);
            if (!result.IsSuccess) return ValidationProblem(result.Errors);
            var blogPost = result.Value!;
            var created = await LoadBlogPostAsync(db, blogPost.Id, cancellationToken);
            return Results.Created(ResourcePath("blog-posts", blogPost.Id, versioned), created!.ToResponse());
        }).Produces<BlogPostResponse>(StatusCodes.Status201Created).Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapPut("/blog-posts/{id:guid}", async (Guid id, BlogPostUpsertRequest request, PapyrusDbContext db, [FromServices] ContentCommandHandlers handlers, CancellationToken cancellationToken) =>
        {
            var result = await handlers.SaveBlogPostAsync(new(id, request.Slug, request.Title, request.Summary, request.ContentMarkdown,
                request.CoverImageUrl, request.ProjectId, request.CategoryId, request.IsFeatured, request.IsPublished,
                request.PublishedAt, Tags(request.TagIds)), cancellationToken);
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
}
