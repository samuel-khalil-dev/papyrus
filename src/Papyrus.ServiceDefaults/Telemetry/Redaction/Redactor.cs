using System.Text.RegularExpressions;

namespace Papyrus.ServiceDefaults.Telemetry.Redaction;

public sealed partial class Redactor
{
    public AuditEvent Redact(AuditEvent value) => value with
    {
        TenantId = Tenant(value.TenantId),
        Actor = Identity(value.Actor),
        Action = SafeToken(value.Action),
        Target = Target(value.Target),
        CorrelationId = Identifier(value.CorrelationId),
        Outcome = SafeToken(value.Outcome),
        TraceId = value.TraceId is null ? null : Identifier(value.TraceId)
    };

    private static string Tenant(string? value) =>
        value is not null && TenantPattern().IsMatch(value) ? value : RedactionPolicy.Redacted;

    private static string Identity(string? value) =>
        value is "admin" or "anonymous" ? value : RedactionPolicy.Redacted;

    private static string Target(string? value)
    {
        if (RedactionPolicy.ContainsSecretMarker(value))
        {
            return RedactionPolicy.Redacted;
        }

        var target = value?.Split('?', '#')[0];
        return target is not null && SafePathPattern().IsMatch(target) ? target : RedactionPolicy.Redacted;
    }

    private static string SafeToken(string? value) =>
        value is not null && SafeTokenPattern().IsMatch(value) ? value : RedactionPolicy.Redacted;

    private static string Identifier(string? value) =>
        value is not null && IdentifierPattern().IsMatch(value) ? value : RedactionPolicy.Redacted;

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,62}$", RegexOptions.CultureInvariant)]
    private static partial Regex TenantPattern();

    [GeneratedRegex("^[a-z0-9._/-]{1,128}$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex SafePathPattern();

    [GeneratedRegex("^[a-z0-9._:-]{1,64}$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex SafeTokenPattern();

    [GeneratedRegex("^[a-z0-9._:-]{1,128}$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex IdentifierPattern();
}
