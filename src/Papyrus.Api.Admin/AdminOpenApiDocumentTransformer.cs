using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Papyrus.Api.Admin;

internal sealed class AdminOpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "AdminApiKey";
    private const string TenantParameterName = "TenantId";

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

        // Register the reusable tenant header parameter.
        components.Parameters ??= new Dictionary<string, IOpenApiParameter>();
        components.Parameters[TenantParameterName] = new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Description = "Tenant identifier used for request isolation. Defaults to main.",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Pattern = "[a-z0-9][a-z0-9-]{0,62}$",
                Default = "main"
            }
        };

        // Require the AdminApiKey scheme at the document level.
        document.Security = [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
            }
        ];

        if (document.Paths is null)
        {
            return Task.CompletedTask;
        }

        // Apply the tenant header to every documented path.
        foreach (var pathItem in document.Paths.Values)
        {
            if (pathItem is not OpenApiPathItem concretePathItem)
            {
                continue;
            }
            concretePathItem.Parameters ??= [];
            concretePathItem.Parameters.Add(new OpenApiParameterReference(TenantParameterName, document));
        }

        return Task.CompletedTask;
    }
}
