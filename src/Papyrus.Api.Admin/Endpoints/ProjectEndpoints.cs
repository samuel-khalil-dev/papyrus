using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Infrastructure;
using static Papyrus.Api.Admin.Endpoints.AdminEndpointSupport;

namespace Papyrus.Api.Admin.Endpoints;

internal static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this RouteGroupBuilder api, bool versioned)
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
}
