using Papyrus.Api.Admin.AdminUi;
using Papyrus.Api.Admin.Media;
using Papyrus.Infrastructure;

namespace Papyrus.Api.Admin.Endpoints;

internal static class MediaEndpoints
{
    private const string LoggerCategory = "Papyrus.Api.Admin.Endpoints.Media";

    public static void MapMediaEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/media", async (LocalMediaService media, CancellationToken cancellationToken) =>
            Results.Ok(await media.ListAsync(cancellationToken)))
            .Produces<IReadOnlyList<MediaFileResult>>();

        api.MapPost("/media", async (HttpRequest request, LocalMediaService media, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            IFormCollection form;
            try
            {
                form = await request.ReadFormAsync(cancellationToken);
            }
            catch (InvalidDataException)
            {
                // A malformed multipart body reaches the form reader before any
                // validation does. Without this guard the request surfaces as an
                // unhandled 500 rather than the 400 the client can act on.
                return Results.Problem(
                    title: "The multipart request body could not be read.",
                    detail: "Send the image as multipart/form-data using a field named 'file'.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var file = form.Files["file"] ?? form.Files.FirstOrDefault();
            if (file is null)
            {
                return Results.Problem(
                    title: "A media file is required.",
                    detail: "Send the image as multipart/form-data using a field named 'file'.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            try
            {
                var uploaded = await media.SaveAsync(file, cancellationToken);
                return Results.Ok(uploaded);
            }
            catch (InvalidOperationException exception)
            {
                // LocalMediaService rejects on seven distinct conditions with
                // operator-facing text. Log the reason and return a stable,
                // client-safe description so exception text is never echoed.
                loggerFactory.CreateLogger(LoggerCategory)
                    .LogWarning(exception, "Rejected a media upload.");
                return Results.Problem(
                    title: "The uploaded file was rejected.",
                    detail: "Accepted uploads are JPEG, PNG, GIF, or WebP raster images of at most 5 MiB, " +
                        "at most 10000x10000 pixels and 40 megapixels, whose file extension matches their content. " +
                        "SVG is not accepted.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }).Produces<MediaUploadResult>().ProducesProblem(StatusCodes.Status400BadRequest)
            .Accepts<IFormFile>("multipart/form-data").DisableAntiforgery();

        api.MapDelete("/media/{fileName}", async (string fileName, LocalMediaService media, PapyrusDbContext db, CancellationToken cancellationToken) =>
        {
            if (await AdminContentSupport.IsMediaReferencedAsync(db, fileName, cancellationToken))
            {
                return Results.Problem(
                    title: "The media file is still referenced.",
                    detail: "Remove the asset from published content and admin drafts before deleting it.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            return await media.DeleteAsync(fileName, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound();
        }).Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status404NotFound);
    }
}
