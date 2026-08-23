using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Papyrus.ServiceDefaults.Telemetry.Redaction;

namespace Papyrus.ServiceDefaults.Telemetry;

public sealed class StructuredAuditEventSink(
    ILogger<StructuredAuditEventSink> logger,
    Redactor? configuredRedactor = null) : IAuditEventSink
{
    private static readonly EventId AuditEventId = new(1001, "AuditEvent");
    private readonly Redactor redactor = configuredRedactor ?? new Redactor();

    public void Record(AuditEvent auditEvent)
    {
        var safeEvent = redactor.Redact(auditEvent);
        using var activity = TelemetryActivitySource.Start("Papyrus.AuditEvent");
        activity?.SetTag("papyrus.audit", true);
        activity?.SetTag("papyrus.tenant_id", safeEvent.TenantId);
        activity?.SetTag("papyrus.audit.action", safeEvent.Action);
        activity?.SetTag("papyrus.audit.target", safeEvent.Target);
        activity?.SetTag("papyrus.audit.outcome", safeEvent.Outcome);

        var level = safeEvent.Outcome.Equals("success", StringComparison.OrdinalIgnoreCase)
            ? LogLevel.Information
            : LogLevel.Warning;
        logger.Log(
            level,
            AuditEventId,
            "Audit event {Action} on {Target} by {Actor} for tenant {TenantId} completed with {Outcome} ({StatusCode}); correlation {CorrelationId}, trace {TraceId}.",
            safeEvent.Action,
            safeEvent.Target,
            safeEvent.Actor,
            safeEvent.TenantId,
            safeEvent.Outcome,
            safeEvent.StatusCode,
            safeEvent.CorrelationId,
            safeEvent.TraceId ?? Activity.Current?.TraceId.ToString());
    }
}
