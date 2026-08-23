namespace Papyrus.Api.Admin.Authentication;

using System.Diagnostics;
using Papyrus.ServiceDefaults.Telemetry;

internal sealed class AdminAuditMiddleware(RequestDelegate next, IAuditEventSink auditEvents)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        if (context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Response.StatusCode >= 400
                || (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)))
            {
                var endpoint = context.GetEndpoint()?.DisplayName ?? "unmatched-endpoint";
                var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "main";
                var outcome = context.Response.StatusCode >= 400 ? "failure" : "success";
                auditEvents.Record(new AuditEvent(
                    tenantId,
                    context.User.Identity?.Name ?? "anonymous",
                    $"http.{context.Request.Method.ToLowerInvariant()}",
                    endpoint,
                    context.TraceIdentifier,
                    outcome,
                    context.Response.StatusCode,
                    Activity.Current?.TraceId.ToString(),
                    DateTimeOffset.UtcNow));
            }
        }
    }
}
