using Papyrus.Api.Admin.Endpoints;

namespace Papyrus.Api.Admin;

internal static class AdminApiEndpoints
{
    public static IEndpointRouteBuilder MapAdminApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").RequireAuthorization().RequireRateLimiting("admin-api");

        MapEndpoints(api, true);

        // Keep the original unversioned surface during the compatibility window.
        var legacyApi = app.MapGroup("/api").RequireAuthorization().RequireRateLimiting("admin-api").ExcludeFromDescription();
        MapEndpoints(legacyApi, false);

        return app;
    }

    private static void MapEndpoints(RouteGroupBuilder api, bool versioned)
    {
        api.MapTaxonomyEndpoints(versioned);
        api.MapProjectEndpoints(versioned);
        api.MapPostEndpoints(versioned);
        api.MapMediaEndpoints();
    }
}
