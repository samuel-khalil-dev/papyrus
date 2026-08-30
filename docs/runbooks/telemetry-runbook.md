# Telemetry Runbook

**Mode:** Diátaxis **Reference + Explanation**. This runbook explains how
Papyrus emits telemetry and gives operators the queries and checks needed to
use it. It covers the shared W3C `Papyrus` ActivitySource, request diagnostics,
structured audit events, redaction, cardinality, sampling, OTLP export, and
the dashboard.

**Owner:** on-call operator / project owner

**Related:** [`alerts.md`](alerts.md), [`incident-runbook.md`](incident-runbook.md),
[`deployment.md`](../guides/deployment.md), and the
[`Papyrus` dashboard](../../ops/dashboards/papyrus-dashboard.json).

## 1. Emission model

Telemetry is implemented in
[`Papyrus.ServiceDefaults/Telemetry`](../../src/Papyrus.ServiceDefaults/Telemetry)
and registered by the shared ServiceDefaults used by Admin, Blog, and
Portfolio. `TelemetryModule` registers the options, redactor, cardinality guard,
sampling policy, and logger-backed audit sink. W3C propagation is forced, so a
request's `traceparent`/`tracestate` context can be followed across hosts.

| Signal | Source and behavior |
| --- | --- |
| `Papyrus.Request` | `ActivityKind.Server`, created by `TelemetryRequestMiddleware` for a sampled request; tags `papyrus.service`, `papyrus.endpoint`, and `http.status_code` |
| `Papyrus.AuditEvent` | `ActivityKind.Internal`, created by `StructuredAuditEventSink` for every audit record; tags `papyrus.audit=true`, tenant, action, target, and outcome |
| Structured audit log | Logger-backed `AuditEvent`, EventId `1001`; emitted even without an OTLP exporter and independently of diagnostic sampling |

The source name is [`TelemetryActivitySource.Name`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryActivitySource.cs:7),
`Papyrus`. Request activities are emitted by the request middleware after the
request completes so the status and endpoint tags are available. Audit
emissions occur on API-key authentication and Admin `/admin` or `/api`
mutation/error paths. Blog and Portfolio use the request seam but are read-only.

An audit record contains `TenantId`, `Actor`, `Action`, `Target`,
`CorrelationId`, `Outcome`, `StatusCode`, `TraceId`, and `OccurredAtUtc`.
There is no persisted audit table and no audit migration.

## 2. Safety controls

### 2.1 Redaction

[`Redactor`](../../src/Papyrus.ServiceDefaults/Telemetry/Redaction/Redactor.cs)
processes the event before logger state or activity tags are created. It is
fail-closed: values outside the allowlist become `[redacted]`.

- Tenant IDs must match `^[a-z0-9][a-z0-9-]{0,62}$`.
- Actors are only `admin` or `anonymous`.
- Targets have query strings and fragments removed and must match the safe path
  pattern; unsafe targets are redacted.
- Actions and outcomes use bounded safe tokens; correlation and trace IDs use
  bounded identifier patterns.
- Secret markers include `api-key`, `apikey`, `authorization`, `bearer`,
  `cookie`, `password`, `secret`, `session`, `token`, `credential`,
  `request-body`, and `set-cookie`.

API keys, cookies, request bodies, and raw credentials must never appear in
activities or structured logs. A `[redacted]` increase is a signal to inspect
the input shape or policy, not to disable fail-closed redaction.

### 2.2 Cardinality

`CardinalityGuard` bounds distinct values independently per diagnostic tag key.
The default `Telemetry:CardinalityLimit` is `100`, clamped to `[1,10000]`.
After the limit, values become `[other]`. The guarded request keys are
`papyrus.service` and `papyrus.endpoint`; the guard prevents route or service
values from overwhelming the backend.

### 2.3 Sampling

`Telemetry:SamplingRate` defaults to `1.0` and is clamped to `[0,1]`.
`1.0` samples every request diagnostic activity and `0.0` samples none. This
policy applies only to `Papyrus.Request` diagnostics. **Audit events are never
sampled**: the logger-backed `AuditEvent` and its `Papyrus.AuditEvent` emission
must survive even when the diagnostic rate is `0.0`.

## 3. Configuration and export

| Setting | Default | Effect |
| --- | --- | --- |
| `Telemetry:CardinalityLimit` | `100` | Maximum distinct values per guarded tag key; runtime clamp `[1,10000]` |
| `Telemetry:SamplingRate` | `1.0` | Diagnostic request sampling rate; audit events unaffected |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | unset | Enables the OTLP exporter when supplied |

Use the deployment platform's configuration mechanism to provide these values.
The OTLP exporter is gated on `OTEL_EXPORTER_OTLP_ENDPOINT`; an unset endpoint
does not disable local structured audit logging. Never treat absence of OTLP
export as absence of an audit event.

## 4. Querying the dashboard and signals

Open [`ops/dashboards/papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json)
in Grafana and select the configured Prometheus datasource. The dashboard
covers request rate, latency, errors, 429 responses, audit volume, redaction,
sampling, cardinality, and application/PostgreSQL health.

Use these dimensions when drilling into traces or normalized metrics:

- `papyrus.service`: `Papyrus.Api.Admin`, `Papyrus.Web.Blog`, or
  `Papyrus.Web.Portfolio`;
- `papyrus.endpoint`: the matched endpoint display name;
- `http.status_code`: response status on `Papyrus.Request`;
- `trace_id`: follow W3C context across a request;
- `papyrus.audit.outcome`, `papyrus.tenant_id`,
  `papyrus.audit.action`, `papyrus.audit.target`, and
  `papyrus.audit.status_code` on audit events.

For Prometheus expressions and the collector metric contract, use
[`alerts.md`](alerts.md). The names such as `papyrus_audit_events_total`,
`papyrus_audit_redacted_total`, `papyrus_cardinality_collapsed_total`, and
`papyrus_diagnostic_sampled_total` are collector-derived views; the application
does not expose application counters with those names.

## 5. Investigate an audit-volume drop

Follow this order for `PapyrusAuditVolumeDrop`:

1. Check the OTLP/collector and dashboard backend. A backend outage normally
   affects all telemetry.
2. Check `admin-api`, `blog`, and `portfolio` health and compare request
   activity. A simultaneous request drop points to hosts or ingress.
3. Inspect structured EventId `1001` records for a rise in `[redacted]` values.
4. Check `Telemetry:SamplingRate`, but do not attribute an audit drop to
   diagnostic sampling: audit events are not sampled.
5. Confirm API-key authentication and Admin mutation/error paths still call the
   audit sink after the last release.
6. Compare with genuine Admin traffic. Reduced use of the Admin UI can be a
   real traffic reduction rather than a telemetry failure.

## 6. Release checks

Before release, verify no secrets are emitted, trace propagation works across
hosts, the dashboard and alert rules use the same collector contract, and the
audit-volume-drop drill completes. Record the result under release gate W7/O4.
