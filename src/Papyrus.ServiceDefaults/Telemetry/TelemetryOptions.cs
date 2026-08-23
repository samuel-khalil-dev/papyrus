namespace Papyrus.ServiceDefaults.Telemetry;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public int CardinalityLimit { get; set; } = 100;

    public double SamplingRate { get; set; } = 1.0;
}
