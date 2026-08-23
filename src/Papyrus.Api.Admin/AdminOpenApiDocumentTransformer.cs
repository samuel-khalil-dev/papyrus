using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Papyrus.Api.Admin;

internal sealed class AdminOpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "AdminApiKey";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var components = document.Components ??= new OpenApiComponents();
        components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-Admin-Api-Key",
            In = ParameterLocation.Header,
            Description = "Admin API key supplied in the X-Admin-Api-Key header."
        };

        if (document.Paths is null)
        {
            return Task.CompletedTask;
        }

        foreach (var pathItem in document.Paths.Values)
        {
            if (pathItem.Operations is null)
            {
                continue;
            }

            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(SchemeName)] = []
                });
            }
        }

        return Task.CompletedTask;
    }
}
