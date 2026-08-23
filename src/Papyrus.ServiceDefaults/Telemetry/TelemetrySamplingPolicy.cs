namespace Papyrus.ServiceDefaults.Telemetry;

using Microsoft.Extensions.Options;

public sealed class TelemetrySamplingPolicy(IOptions<TelemetryOptions> options)
{
    private readonly double rate = Math.Clamp(options.Value.SamplingRate, 0, 1);

    public bool ShouldSampleDiagnostic() => rate >= 1 || rate > 0 && Random.Shared.NextDouble() < rate;
}
