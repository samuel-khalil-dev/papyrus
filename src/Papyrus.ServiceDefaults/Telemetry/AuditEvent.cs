namespace Papyrus.ServiceDefaults.Telemetry;

public sealed record AuditEvent(
    string TenantId,
    string Actor,
    string Action,
    string Target,
    string CorrelationId,
    string Outcome,
    int StatusCode,
    string? TraceId,
    DateTimeOffset OccurredAtUtc);

public interface IAuditEventSink
{
    void Record(AuditEvent auditEvent);
}
