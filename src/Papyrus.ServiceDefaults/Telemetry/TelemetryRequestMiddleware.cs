using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Papyrus.ServiceDefaults.Telemetry;

public sealed class TelemetryRequestMiddleware(
    RequestDelegate next,
    TelemetrySamplingPolicy sampling,
    CardinalityGuard cardinality)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = sampling.ShouldSampleDiagnostic()
            ? TelemetryActivitySource.Start("Papyrus.Request", ActivityKind.Server)
            : null;
        activity?.SetTag("papyrus.service", cardinality.GetValue(
            "papyrus.service", context.RequestServices.GetRequiredService<IHostEnvironment>().ApplicationName));

        await next(context);

        activity?.SetTag("http.status_code", context.Response.StatusCode);
        activity?.SetTag("papyrus.endpoint", cardinality.GetValue(
            "papyrus.endpoint", context.GetEndpoint()?.DisplayName ?? "unmatched-endpoint"));
    }
}
