using System.Diagnostics;

namespace Papyrus.ServiceDefaults.Telemetry;

public static class TelemetryActivitySource
{
    public const string Name = "Papyrus";

    public static ActivitySource Source { get; } = new(Name);

    public static Activity? Start(string name, ActivityKind kind = ActivityKind.Internal) =>
        Source.StartActivity(name, kind);
}
