using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Infrastructure;
using static Papyrus.Api.Admin.Endpoints.AdminEndpointSupport;

namespace Papyrus.Api.Admin.Endpoints;

internal static class TaxonomyEndpoints
{
    public static void MapTaxonomyEndpoints(this RouteGroupBuilder api, bool versioned)
    {
        MapCategoryEndpoints(api, versioned);
        MapTagEndpoints(api, versioned);
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
                ? Results.Created(ResourcePath("categories", result.Value!.Id, versioned), result.Value.ToResponse())
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
                ? Results.Created(ResourcePath("tags", result.Value!.Id, versioned), result.Value.ToResponse())
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
}
