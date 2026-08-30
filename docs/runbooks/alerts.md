# Papyrus Alert Families

**Mode:** Diátaxis **Reference**. These alert families map operational alerts
to the actual Papyrus telemetry seam and the Docker Compose health topology.
Thresholds are starting defaults and should be tuned against a production
baseline without changing the signal mapping.

The dashboard is [`ops/dashboards/papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json).
Telemetry behavior and investigation steps are in
[`telemetry-runbook.md`](telemetry-runbook.md); response procedures are in
[`incident-runbook.md`](incident-runbook.md).

## Alert families

| Alert | Signal / expression shape | Threshold / `for` | Severity |
| --- | --- | --- | --- |
| `PapyrusRequestLatencyHigh` | p95 of `http_server_request_duration_seconds_bucket`, grouped by `papyrus_service` | `> 1` second for 10m | SEV-2 |
| `PapyrusRequest5xxRate` | 5xx request count divided by all request count, grouped by `papyrus_service` | `> 1%` for 10m; page immediately for any Admin 5xx | SEV-2 / Admin SEV-1 |
| `PapyrusRateLimit429` | Request count with `http_response_status_code="429"` | `> 0` responses/s for 10m | SEV-3 |
| `PapyrusAuditVolumeDrop` | Audit activity increase in 15m compared with the preceding hour | Less than half, for 15m, with at least 10 prior-hour events | SEV-2 |
| `PapyrusDiagnosticSamplingSuppressed` | Collector view of sampled diagnostics divided by request count | `< 0.1` for 10m while configured sampling is at least `0.1` | SEV-3 |
| `PapyrusRedactionActivity` | Collector count of structured audit fields equal to `[redacted]` | `> 10` fields/s for 10m or 5x prior-hour increase | SEV-3 |
| `PapyrusCardinalityGuardCollapse` | Collector count of request attributes equal to `[other]`, by tag key | `> 0` values/s for 10m | SEV-3 |
| `PapyrusApplicationDown` | `min by (job) (up{job=~"admin-api|blog|portfolio"})` | `< 1` for 2m | SEV-1 |
| `PapyrusPostgresDown` | `min(up{job="postgres"})` | `< 1` for 2m | SEV-1 |

The request signals come from `Papyrus.Request` and its service, endpoint, and
status tags. The audit signal comes from `Papyrus.AuditEvent` and structured
EventId `1001`. Audit events are independent of diagnostic sampling, so an
audit-volume alert must not be dismissed as a low `Telemetry:SamplingRate`.

## Collector-boundary metric contract

The application emits activities and structured logs, not Prometheus counters
directly. The following are **collector-derived recording views**, not
application counters, and no application instrumentation with these names
exists:

- `http_server_request_duration_seconds_count` and `_bucket` normalize the
  HTTP server duration signal and carry `papyrus_service` and
  `http_response_status_code`. They back request rate, latency, 5xx, and 429
  alerts.
- `papyrus_audit_events_total` counts structured `AuditEvent` records (EventId
  `1001`) and may be grouped by `papyrus_audit_outcome`.
- `papyrus_audit_redacted_total` counts EventId `1001` records whose redacted
  allowlist fields equal `[redacted]`.
- `papyrus_cardinality_collapsed_total` counts request attributes equal to
  `[other]`, grouped by `papyrus_tag_key`.
- `papyrus_diagnostic_sampled_total` is a collector view of sampled request
  diagnostics.

If a collector uses different names, change recording rules at the collector
boundary and update the dashboard together with these rules. Do not invent
application labels or a second naming scheme for the same signal.

## Response mapping

- Request latency, errors, and 429s: inspect `Papyrus.Request` by service and
  endpoint, then follow the [incident triage](incident-runbook.md#3-triage).
- Audit volume, redaction, cardinality, and sampling: follow the
  [telemetry investigation](telemetry-runbook.md#5-investigate-an-audit-volume-drop).
- Application or PostgreSQL health: follow the corresponding Compose incident
  procedure in [`incident-runbook.md`](incident-runbook.md).
