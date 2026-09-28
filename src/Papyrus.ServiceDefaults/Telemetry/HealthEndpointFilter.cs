using Microsoft.AspNetCore.Http;

namespace Papyrus.ServiceDefaults.Telemetry;

/// The health and aliveness probe paths, and the tracing filter that keeps probe
/// traffic out of the trace stream. Probe requests are high-frequency and carry
/// no diagnostic value, but they would otherwise dominate the exported spans.
public static class HealthEndpointFilter
{
    public const string HealthEndpointPath = "/health";

    public const string AlivenessEndpointPath = "/alive";

    public static bool IsExcluded(PathString requestPath) =>
        requestPath.StartsWithSegments(HealthEndpointPath)
        || requestPath.StartsWithSegments(AlivenessEndpointPath);
}
