namespace Papyrus.ServiceDefaults.Telemetry;

public sealed class CardinalityOptions
{
    public const string SectionName = TelemetryOptions.SectionName;

    public int Limit { get; set; } = 100;
}
