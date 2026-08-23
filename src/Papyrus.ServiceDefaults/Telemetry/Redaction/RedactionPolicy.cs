namespace Papyrus.ServiceDefaults.Telemetry.Redaction;

public static class RedactionPolicy
{
    public const string Redacted = "[redacted]";

    private static readonly string[] SecretMarkers =
    [
        "api-key", "apikey", "authorization", "bearer", "cookie", "password",
        "secret", "session", "token", "credential", "request-body", "set-cookie"
    ];

    public static bool ContainsSecretMarker(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && SecretMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
