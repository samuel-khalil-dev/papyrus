using Papyrus.Api.Admin.AdminUi;
using Papyrus.Api.Admin.Media;
using Papyrus.Infrastructure;

namespace Papyrus.Api.Admin.Endpoints;

internal static class MediaEndpoints
{
    public static void MapMediaEndpoints(this RouteGroupBuilder api)
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
}
