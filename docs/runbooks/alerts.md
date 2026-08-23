# Papyrus Alert Families

**Scope:** Prometheus alert rules for the Papyrus web hosts and PostgreSQL. The
rules below are intentionally expressed against the normalized metrics produced
by the OpenTelemetry/Prometheus pipeline. The application emits activities and
structured logs, not Prometheus counters directly; the collector must preserve
the activity names and attributes documented in
[`telemetry-runbook.md`](telemetry-runbook.md).

The portable dashboard is
[`ops/dashboards/papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json).
Thresholds are operational defaults and should be tuned against a production
baseline without changing the underlying signal mapping.

## Alert rules

| Alert | PromQL expression | Threshold / `for` | Severity | Exact telemetry signal |
| --- | --- | --- | --- | --- |
| `PapyrusRequestLatencyHigh` | `histogram_quantile(0.95, sum by (le, papyrus_service) (rate(traces_spanmetrics_latency_bucket{span_name="Papyrus.Request"}[5m])))` | `> 1` second for 10m | SEV-2 | S30.6 `Papyrus.Request` server activity duration, tagged `papyrus.service`; the 1000 ms p95 budget is defined in the performance baseline. |
| `PapyrusRequest5xxRate` | `sum by (papyrus_service) (rate(traces_spanmetrics_calls_total{span_name="Papyrus.Request",http_status_code=~"5.."}[5m])) / clamp_min(sum by (papyrus_service) (rate(traces_spanmetrics_calls_total{span_name="Papyrus.Request"}[5m])), 1e-9)` | `> 0.01` for 10m; additionally page immediately when `papyrus_service="Papyrus.Api.Admin"` and any 5xx is present | SEV-2 (SEV-1 for admin outage) | S30.6 `Papyrus.Request` `http.status_code` activity tag, normalized as `http_status_code`. |
| `PapyrusRateLimit429` | `sum by (papyrus_service) (rate(traces_spanmetrics_calls_total{span_name="Papyrus.Request",http_status_code="429"}[5m]))` | `> 0` responses/s for 10m | SEV-3 | S30.6 `Papyrus.Request` `http.status_code=429`; this is the Admin per-IP rate-limit response signal, not a generic error. |
| `PapyrusAuditVolumeDrop` | `sum(increase(traces_spanmetrics_calls_total{span_name="Papyrus.AuditEvent"}[15m])) < 0.5 * sum(increase(traces_spanmetrics_calls_total{span_name="Papyrus.AuditEvent"}[15m] offset 1h))` | true for 15m, and the prior-hour comparison must be at least 10 events | SEV-2 | S30.6 `Papyrus.AuditEvent` activity plus EventId `1001`/`AuditEvent` structured log. Audit emission is not subject to S30.7 diagnostic sampling. |
| `PapyrusDiagnosticSamplingSuppressed` | `sum(rate(traces_spanmetrics_calls_total{span_name="Papyrus.Request"}[5m])) / clamp_min(sum(rate(http_server_request_duration_seconds_count[5m])), 1e-9)` | `< 0.1` for 10m while `Telemetry:SamplingRate >= 0.1` | SEV-3 | S30.7 `TelemetrySamplingPolicy.ShouldSampleDiagnostic()` and the presence/absence of S30.6 `Papyrus.Request` activities. This detects unexpected suppression; it does not alert when configured sampling is intentionally `0`. |
| `PapyrusRedactionActivity` | `sum(rate(papyrus_audit_redacted_fields_total[5m]))` | `> 10` fields/s for 10m, or a 5x increase over the preceding hour | SEV-3 | S30.7 `Redactor` fail-closed output (`[redacted]`) at the `StructuredAuditEventSink` seam. `papyrus_audit_redacted_fields_total` is a collector-derived metric from the structured `AuditEvent` log fields; no application counter is implied. |
| `PapyrusCardinalityGuardCollapse` | `sum by (papyrus_tag_key) (rate(papyrus_cardinality_other_total[5m]))` | `> 0` collapsed values/s for 10m | SEV-3 | S30.7 `CardinalityGuard.GetValue()` returning `[other]` for `papyrus.service` or `papyrus.endpoint`. `papyrus_cardinality_other_total` is a collector-derived metric from those activity attributes. |
| `PapyrusApplicationDown` | `min by (job) (up{job=~"admin-api|blog|portfolio"})` | `< 1` for 2m | SEV-1 | Platform scrape health for the three hosts, corresponding to the `/health` and `/alive` operational probes documented by the incident runbook. |
| `PapyrusPostgresDown` | `min(up{job="postgres"})` | `< 1` for 2m | SEV-1 | Platform scrape health for PostgreSQL 17 and its health probe. |

### Collector-boundary metric contract

The following names are the portable Prometheus representation used by the
expressions above:

- `traces_spanmetrics_calls_total` and `traces_spanmetrics_latency_bucket`
  preserve `Papyrus.Request` / `Papyrus.AuditEvent` names and the request status,
  service, and endpoint attributes.
- `papyrus_audit_redacted_fields_total` counts structured `AuditEvent` records
  whose redacted allowlist fields equal `[redacted]`. It is derived from the
  logger-backed EventId `1001` record; it is not a new application signal.
- `papyrus_cardinality_other_total` counts request activity attributes whose
  value is `[other]`, grouped by the affected tag key. It is derived from the
  output of `CardinalityGuard`, whose configured limit is
  `Telemetry:CardinalityLimit`.
- `http_server_request_duration_seconds_count` is the all-request baseline used
  only to compare sampled `Papyrus.Request` diagnostic activities.

If the deployment uses different collector metric names, update recording rules
or the dashboard queries at the collector boundary. Do not replace these rules
with labels that are not present in the telemetry source.

## Response links

Every alert maps to the following response material:

- **Telemetry investigation:** [`telemetry-runbook.md`](telemetry-runbook.md),
  especially [querying telemetry](telemetry-runbook.md#4-querying-telemetry),
  [cardinality and sampling](telemetry-runbook.md#3-cardinality-and-sampling),
  and [investigating an audit-volume drop](telemetry-runbook.md#5-investigating-an-audit-volume-drop).
- **Incident handling:** [`incident-runbook.md`](incident-runbook.md), especially
  [detection](incident-runbook.md#2-detection),
  [triage](incident-runbook.md#3-triage),
  [containment](incident-runbook.md#4-containment), and
  [recovery](incident-runbook.md#5-recovery).

For `PapyrusAuditVolumeDrop`, follow the audit-drop sequence in order: verify
the telemetry backend, compare request activity, inspect `[redacted]` fields,
confirm that diagnostic sampling was not mistaken for audit sampling, verify
the audit emission paths, and finally check whether traffic genuinely dropped.
