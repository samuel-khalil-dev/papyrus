# Telemetry Runbook

**Scope:** how to operate and query Papyrus telemetry. This covers the shared
`Papyrus` W3C ActivitySource, structured audit events, redaction, cardinality
guards, diagnostic sampling, and how to investigate an audit-volume drop or other
telemetry signals. It is a release-gate artifact for **W7** (observability and
alerting) and **O4** (telemetry/audit signal evidence).

**Owner:** on-call operator / project owner
**Related documents:** [`incident-runbook.md`](incident-runbook.md),
[`audit-register.md`](../security/audit-register.md)

For Prometheus alert expressions and response ownership, see the
[`alerts.md`](alerts.md) alert-family runbook. The portable Grafana dashboard is
[`ops/dashboards/papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json).

---

## 1. Telemetry architecture

Telemetry is implemented in
[`src/Papyrus.ServiceDefaults/Telemetry`](../../src/Papyrus.ServiceDefaults/Telemetry)
and wired through the shared request seam used by Admin, Blog, and Portfolio.

| Component | Purpose |
| --- | --- |
| [`TelemetryActivitySource`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryActivitySource.cs) | Shared `Papyrus` `ActivitySource`; W3C trace propagation |
| [`TelemetryRequestMiddleware`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryRequestMiddleware.cs) | Per-request `Papyrus.Request` activity with service/endpoint/status tags |
| [`StructuredAuditEventSink`](../../src/Papyrus.ServiceDefaults/Telemetry/StructuredAuditEventSink.cs) | Logger-backed structured audit sink; emits `Papyrus.AuditEvent` activities |
| [`Redactor`](../../src/Papyrus.ServiceDefaults/Telemetry/Redaction/Redactor.cs) | Fail-closed redaction of audit fields |
| [`CardinalityGuard`](../../src/Papyrus.ServiceDefaults/Telemetry/CardinalityGuard.cs) | Bounded diagnostic tag cardinality |
| [`TelemetrySamplingPolicy`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetrySamplingPolicy.cs) | Configurable diagnostic sampling rate |

### 1.1 ActivitySource and trace propagation

- The shared source name is **`Papyrus`**
  ([`TelemetryActivitySource.Name`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryActivitySource.cs:7)).
- W3C trace propagation is forced
  ([`ConfigureTracePropagation()`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryModule.cs:23)),
  so `traceparent`/`tracestate` headers are honored and emitted.
- Two activity kinds are produced:
  - **`Papyrus.Request`** (`ActivityKind.Server`) — one per sampled HTTP request,
    tagged with `papyrus.service`, `papyrus.endpoint`, and `http.status_code`.
  - **`Papyrus.AuditEvent`** (`ActivityKind.Internal`) — one per audit event,
    tagged with `papyrus.audit=true`, `papyrus.tenant_id`, `papyrus.audit.action`,
    `papyrus.audit.target`, and `papyrus.audit.outcome`.

### 1.2 Structured audit events

Audit events are emitted through
[`StructuredAuditEventSink`](../../src/Papyrus.ServiceDefaults/Telemetry/StructuredAuditEventSink.cs)
as structured log entries (EventId `1001`, name `AuditEvent`) and as
`Papyrus.AuditEvent` activities. Each event carries:

- `papyrus.tenant_id` — tenant scope
- `papyrus.audit.action` — the action performed
- `papyrus.audit.target` — the target resource
- `papyrus.audit.outcome` — `success` (logged at Information) or failure
  (logged at Warning)
- `papyrus.audit.status_code` — HTTP status
- correlation id and trace id for cross-referencing

Audit events are emitted for API-key authentication and Admin API
mutations/errors. There is **no persisted audit table or migration**; audit
records live in the structured log/telemetry backend.

---

## 2. Redaction behavior

Redaction is **fail-closed**: any field that does not match a strict allowlist
pattern is replaced with `[redacted]`
([`RedactionPolicy.Redacted`](../../src/Papyrus.ServiceDefaults/Telemetry/Redaction/RedactionPolicy.cs:5)).

- **Tenant IDs** must match `^[a-z0-9][a-z0-9-]{0,62}$`.
- **Actor** is preserved only for `admin` or `anonymous`; anything else is
  redacted.
- **Targets** are stripped of query/fragment and must match a safe path pattern;
  any target containing a secret marker is redacted.
- **Secret markers** include `api-key`, `apikey`, `authorization`, `bearer`,
  `cookie`, `password`, `secret`, `session`, `token`, `credential`,
  `request-body`, and `set-cookie`
  ([`RedactionPolicy.SecretMarkers`](../../src/Papyrus.ServiceDefaults/Telemetry/Redaction/RedactionPolicy.cs:7)).
- **Actions, outcomes, correlation ids, and trace ids** must match strict token
  patterns or are redacted.

API keys, cookies, request bodies, and raw credentials are never emitted.

---

## 3. Cardinality and sampling

### 3.1 Cardinality guard

Diagnostic tags (`papyrus.service`, `papyrus.endpoint`) are passed through
[`CardinalityGuard`](../../src/Papyrus.ServiceDefaults/Telemetry/CardinalityGuard.cs),
which caps the number of distinct values per tag key. Values beyond the limit are
collapsed to `[other]`. The default limit is `100`
([`TelemetryOptions.CardinalityLimit`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryOptions.cs:7)),
clamped to `[1, 10_000]`. This prevents unbounded tag cardinality from
overwhelming the telemetry backend.

### 3.2 Diagnostic sampling

Request diagnostic activities are sampled by
[`TelemetrySamplingPolicy`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetrySamplingPolicy.cs)
using `Telemetry:SamplingRate` (default `1.0`, clamped to `[0, 1]`).

- `SamplingRate = 1.0` — sample all requests.
- `SamplingRate = 0.0` — sample no request diagnostics.
- **Audit events are NOT sampled.** Audit logging survives a `0%` diagnostic
  sampling rate; only the per-request `Papyrus.Request` diagnostic activity is
  suppressed. This is verified by telemetry tests (S30.7).

---

## 4. Querying telemetry

### 4.1 Request traces

Query the `Papyrus` ActivitySource for `Papyrus.Request` activities. Useful
filters:

- `papyrus.service` — which host (`Papyrus.Api.Admin`, `Papyrus.Web.Blog`,
  `Papyrus.Web.Portfolio`).
- `papyrus.endpoint` — the matched endpoint display name.
- `http.status_code` — response status; filter for `5xx` to find errors.
- `trace_id` — follow a single request across hosts via W3C propagation.

### 4.2 Audit events

Query for `Papyrus.AuditEvent` activities or the `AuditEvent` structured log
entries (EventId `1001`). Useful filters:

- `papyrus.audit.outcome` — `success` vs failure.
- `papyrus.tenant_id` — tenant-scoped activity.
- `papyrus.audit.action` / `papyrus.audit.target` — what was done to what.
- `papyrus.audit.status_code` — HTTP status of the audited operation.

### 4.3 Cross-referencing

Each audit event carries a correlation id and trace id. Use the trace id to join
an audit event to the originating `Papyrus.Request` activity and any downstream
spans.

---

## 5. Investigating an audit-volume drop

A sudden drop in audit event volume is a telemetry health signal. Investigate in
this order:

1. **Check the telemetry backend.** Is the exporter/collector healthy? A backend
   outage produces a drop in *all* telemetry, not just audit events.
2. **Check the hosts.** Are `admin-api`, `blog`, and `portfolio` healthy and
   emitting request activities? If request traces also dropped, the hosts or
   ingress are the problem.
3. **Check redaction.** If a field pattern changed and now redacts most values,
   audit events still emit but with `[redacted]` fields. Verify the redaction
   allowlist against the current source.
4. **Check sampling.** Confirm `Telemetry:SamplingRate` was not accidentally set
   to `0.0`. Note that audit events are **not** sampled, so a sampling change
   alone should not drop audit events — if it did, that is a bug.
5. **Check the audit emission path.** Confirm API-key authentication and Admin
   mutation/error paths still call the audit sink. A code change that bypasses
   the sink would reduce audit volume.
6. **Check for a traffic drop.** A genuine reduction in admin traffic (e.g. the
   admin UI is not being used) legitimately reduces audit volume. Correlate with
   request trace volume before treating it as an incident.

---

## 6. Configuration reference

| Setting | Default | Effect |
| --- | --- | --- |
| `Telemetry:CardinalityLimit` | `100` | Max distinct values per diagnostic tag key |
| `Telemetry:SamplingRate` | `1.0` | Diagnostic request sampling rate `[0, 1]`; audit events unaffected |

Configuration is bound from the `Telemetry` section
([`TelemetryOptions.SectionName`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetryOptions.cs:5)).

---

## 7. Runbook exercise (release gate W7/O4)

Before release, record:

- A redaction review confirming no secrets/credentials are emitted.
- Trace propagation across hosts (W3C `traceparent`).
- A dashboard/alert configuration for the signals in
  [`incident-runbook.md`](incident-runbook.md#21-alert-sources).
- An audit-volume-drop investigation drill using the steps in section 5.

Record the outcome as **W7** and **O4** evidence in
[`release-gates.md`](../architecture/release-gates.md).
