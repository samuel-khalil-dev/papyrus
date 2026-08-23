using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Papyrus.ServiceDefaults.Telemetry;

public sealed class CardinalityGuard(IOptions<TelemetryOptions> options)
{
    private readonly int limit = Math.Clamp(options.Value.CardinalityLimit, 1, 10_000);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> values = new(StringComparer.Ordinal);

    public string GetValue(string key, string? value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value) ? "unknown" : value;
        var keyValues = values.GetOrAdd(key, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
        if (keyValues.ContainsKey(safeValue) || keyValues.Count < limit && keyValues.TryAdd(safeValue, 0))
        {
            return safeValue;
        }

        return "[other]";
    }
}
