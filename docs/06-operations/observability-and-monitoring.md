# Observability & Monitoring

## Application-Level Observability (Current)

Papyrus runs a real OTLP-based telemetry pipeline today: an OTLP collector feeding Prometheus and Grafana, with a dedicated `Papyrus` `ActivitySource`, controlled sampling (`Telemetry:SamplingRate`), a cardinality limit (`Telemetry:CardinalityLimit`), and ten alert families defined below. `EventId 1001` is intentionally never sampled. The Grafana dashboard definition is tracked at [`observability/dashboards/papyrus-dashboard.json`](../observability/dashboards/papyrus-dashboard.json), and the deployable stack is tracked at [`ops/observability/`](../../ops/observability).

Do not describe this stack as absent, and do not propose replacing it — Prometheus and Grafana are not "too heavy" for this platform; they are already running successfully on it.

### Where the stack lives

The stack is **tracked configuration, not host state**. It is deployed by the `deploy` job in [release.yml](../../.github/workflows/release.yml) with `docker compose -f docker-compose.yml -f ops/observability/docker-compose.observability.yml up -d` and never hand-placed on the VPS. Any change to the collector, alert rules, or Grafana provisioning belongs in `ops/observability/` and ships through the pipeline.

| File | Role |
| --- | --- |
| [`ops/observability/docker-compose.observability.yml`](../../ops/observability/docker-compose.observability.yml) | Collector, Prometheus, Grafana, and blackbox-exporter, all pinned by digest and bound to loopback |
| [`ops/observability/collector/config.yaml`](../../ops/observability/collector/config.yaml) | OTLP receivers and the derivation of every collector-boundary metric |
| [`ops/observability/prometheus/prometheus.yml`](../../ops/observability/prometheus/prometheus.yml) | Scrape configuration, including the `up` targets the availability alerts require |
| [`ops/observability/prometheus/papyrus-alerts.yml`](../../ops/observability/prometheus/papyrus-alerts.yml) | The alert families below, transcribed from this document |
| [`ops/observability/grafana/provisioning/`](../../ops/observability/grafana/provisioning) | Datasource and dashboard providers; the dashboard itself is mounted from `docs/` |

The applications receive `OTEL_EXPORTER_OTLP_ENDPOINT` from compose. The release workflow sets it to `http://otel-collector:4317`; a deployment that does not run the stack leaves it empty, which keeps the exporter disabled and structured audit logging intact.

## Telemetry contracts (Current)

Telemetry is implemented in `src/Papyrus.ServiceDefaults/Telemetry` and registered by the shared ServiceDefaults used by Admin, Blog, and Portfolio. `TelemetryModule` registers the options, redactor, cardinality guard, sampling policy, and logger-backed audit sink. W3C propagation is forced, so a request's `traceparent`/`tracestate` context can be followed across hosts.

| Signal | Source and behavior |
| --- | --- |
| `Papyrus.Request` | `ActivityKind.Server`, created by `TelemetryRequestMiddleware` for a sampled request; tags `papyrus.service`, `papyrus.endpoint`, and `http.status_code` |
| `Papyrus.AuditEvent` | `ActivityKind.Internal`, created by `StructuredAuditEventSink` for every audit record; tags `papyrus.audit=true`, tenant, action, target, and outcome |
| Structured audit log | Logger-backed `AuditEvent`, EventId `1001`; emitted even without an OTLP exporter and independently of diagnostic sampling |

The source name is `Papyrus`. Request activities are emitted by the request middleware after the request completes so the status and endpoint tags are available. Audit emissions occur on API-key authentication and Admin root or `/api` mutation/error paths. Blog and Portfolio use the request seam but are read-only.

An audit record contains `TenantId`, `Actor`, `Action`, `Target`, `CorrelationId`, `Outcome`, `StatusCode`, `TraceId`, and `OccurredAtUtc`. There is no persisted audit table and no audit migration.

### Redaction

The `Redactor` processes the event before logger state or activity tags are created. It is fail-closed: values outside the allowlist become `[redacted]`.

- Tenant IDs must match `^[a-z0-9][a-z0-9-]{0,62}$`.
- Actors are only `admin` or `anonymous`.
- Targets have query strings and fragments removed and must match the safe path pattern; unsafe targets are redacted.
- Actions and outcomes use bounded safe tokens; correlation and trace IDs use bounded identifier patterns.
- Secret markers include `api-key`, `apikey`, `authorization`, `bearer`, `cookie`, `password`, `secret`, `session`, `token`, `credential`, `request-body`, and `set-cookie`.

API keys, cookies, request bodies, and raw credentials must never appear in activities or structured logs. A `[redacted]` increase is a signal to inspect the input shape or policy, not to disable fail-closed redaction.

### Cardinality

`CardinalityGuard` bounds distinct values independently per diagnostic tag key. The default `Telemetry:CardinalityLimit` is `100`, clamped to `[1,10000]`. After the limit, values become `[other]`. The guarded request keys are `papyrus.service` and `papyrus.endpoint`; the guard prevents route or service values from overwhelming the backend.

### Sampling

`Telemetry:SamplingRate` defaults to `1.0` and is clamped to `[0,1]`. `1.0` samples every request diagnostic activity and `0.0` samples none. This policy applies only to `Papyrus.Request` diagnostics. **Audit events are never sampled**: the logger-backed `AuditEvent` and its `Papyrus.AuditEvent` emission must survive even when the diagnostic rate is `0.0`.

### Configuration and export

| Setting | Default | Effect |
| --- | --- | --- |
| `Telemetry:CardinalityLimit` | `100` | Maximum distinct values per guarded tag key; runtime clamp `[1,10000]` |
| `Telemetry:SamplingRate` | `1.0` | Diagnostic request sampling rate; audit events unaffected |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | unset | Enables the OTLP exporter when supplied |

Use the deployment platform's configuration mechanism to provide these values. The OTLP exporter is gated on `OTEL_EXPORTER_OTLP_ENDPOINT`; an unset endpoint does not disable local structured audit logging. Never treat absence of OTLP export as absence of an audit event.

## Live alert families (Current)

The dashboard definition is tracked at [`observability/dashboards/papyrus-dashboard.json`](../observability/dashboards/papyrus-dashboard.json). Telemetry behavior and investigation steps are in [observability-and-monitoring.md](observability-and-monitoring.md); response procedures are in [runbooks.md](runbooks.md).

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
| `PapyrusAuditSignalLost` | `absent(papyrus_audit_events_total) or absent(papyrus_audit_log_records_total)` | Both audit signals absent for 15m | SEV-2 |

The two availability alerts read `up`, which the OTLP path does not produce. They are satisfied by **blackbox probes**: Prometheus scrapes blackbox-exporter, which probes each host's anonymous `/health` over HTTP and Postgres over TCP, and the relabelled probe results become the `up{job=...}` series the rules evaluate. The probe targets carry the `job` label the alert expressions match on. Because `/health` is excluded from tracing, probing does not distort the diagnostic-sampling ratio.

`PapyrusAuditSignalLost` is not one of the originally documented nine families. It exists because the audit families depend on a signal that can vanish silently: if both the audit span and the audit log record stop reaching the collector, `PapyrusAuditVolumeDrop` cannot fire — there is simply nothing to compare. This alert is what makes that failure visible.

The request signals come from `Papyrus.Request` and its service, endpoint, and status tags. The audit signal comes from `Papyrus.AuditEvent` and structured EventId `1001`. Audit events are independent of diagnostic sampling, so an audit-volume alert must not be dismissed as a low `Telemetry:SamplingRate`.

### Collector-boundary metric contract

The application emits activities and structured logs, not Prometheus counters directly. The following are **collector-derived recording views**, not application counters, and no application instrumentation with these names exists:

- `http_server_request_duration_seconds_count` and `_bucket` normalize the HTTP server duration signal and carry `papyrus_service` and `http_response_status_code`. They back request rate, latency, 5xx, and 429 alerts. The Prometheus exporter exposes resource attributes through `target_info` rather than as labels, so the collector's `transform/metric_labels` processor copies `service.name` onto each data point to produce the `papyrus_service` label the contract requires.
- `papyrus_audit_events_total` counts `Papyrus.AuditEvent` spans and may be grouped by `papyrus_audit_outcome`.
- `papyrus_audit_log_records_total` counts the same audit records from the logger side (EventId `1001`). It is the availability-independent view: the span can be dropped by sampling, the log record cannot.
- `papyrus_audit_redacted_total` counts audit spans whose redacted allowlist fields equal `[redacted]`.
- `papyrus_cardinality_collapsed_total` counts request activities with a `[other]` guard value, grouped by `papyrus_tag_key`.
- `papyrus_diagnostic_sampled_total` counts `Papyrus.Request` spans the sampling policy admitted.

If the collector uses different names, change the count connector in `ops/observability/collector/config.yaml` and update the dashboard and the alert rules together. Do not invent application labels or a second naming scheme for the same signal. `PipelineWiringTests.Alert_rules_only_query_metrics_the_collector_derives` fails if a rule queries a name the collector does not derive.

## Production observability verification checklist (W7 / O4)

This checklist documents the production observability platform required to close release gates **W7 (Observability and alerting)** and **O4 (Telemetry/audit signal evidence)**.

### Required production infrastructure

| Component | Purpose | Required for gate |
| --- | --- | --- |
| OTLP collector (e.g. OpenTelemetry Collector) | Receives `Papyrus.Request` and `Papyrus.AuditEvent` activities from Admin, Blog, and Portfolio via `OTEL_EXPORTER_OTLP_ENDPOINT`; normalizes to Prometheus recording views | W7, O4 |
| Prometheus (or compatible) | Evaluates the alert expressions in the alert families above | W7 |
| Grafana | Serves the Papyrus dashboard (Grafana/Prometheus datasource; dashboard definition tracked at [`observability/dashboards/papyrus-dashboard.json`](../observability/dashboards/papyrus-dashboard.json)) | W7, O4 |
| Retention policy | Retains audit/telemetry signals per the deployment's compliance window | O4 |

### Verification checklist (close W7 and O4)

Execute each item against the **production** deployment and record the result (evidence artifact, date, and outcome) under release gate W7/O4 in [release-gates.md](../04-engineering-standards/release-gates.md). Do not mark an item complete from local or staging evidence alone.

#### Redaction-output inspection (W7)

- [ ] Generate a request that triggers an audit event (e.g. an Admin API-key mutation) and confirm the emitted activity/log contains **no** API key, cookie, request body, or raw credential.
- [ ] Confirm a `[redacted]` value appears for any field outside the allowlist and that the redaction is fail-closed (no partial or raw values leak).
- [ ] Record the sanitized redaction output as evidence.

#### Cross-host trace correlation (W7)

- [ ] Issue a request that flows across hosts (e.g. a public page that references Admin media) and confirm the W3C `traceparent`/`tracestate` context is preserved across Admin, Blog, and Portfolio.
- [ ] Confirm the trace can be followed end-to-end in the collector/dashboard using `trace_id`.

#### Alert firing and recovery (W7)

- [ ] Trigger each alert family above (or a safe subset) and confirm it fires at the configured threshold.
- [ ] Confirm the alert recovers (clears) when the signal returns to normal.
- [ ] Record the alert firing/recovery evidence (e.g. alert history export).

#### Audit-volume-drop runbook exercise (W7)

- [ ] Execute the [observability-and-monitoring.md](observability-and-monitoring.md) audit-volume-drop investigation steps against a simulated or real audit-volume drop.
- [ ] Confirm the `PapyrusAuditVolumeDrop` alert fires and the runbook steps identify the root cause.
- [ ] Record the drill outcome.

#### Live dashboard data (O4)

- [ ] Confirm the Grafana dashboard renders live data for request rate, latency, 5xx, 429, audit volume, redaction, sampling, cardinality, and application/PostgreSQL health.
- [ ] Record a sanitized dashboard screenshot or export as evidence.

#### Alert delivery/recovery (O4)

- [ ] Confirm alert notifications are delivered to the on-call channel and that recovery notifications are sent.
- [ ] Record the delivery evidence.

#### Retention configuration (O4)

- [ ] Confirm the retention policy is configured for audit/telemetry signals per the deployment's compliance window.
- [ ] Record the retention configuration as evidence.

### Recording the result

After all checklist items pass, update the W7 and O4 sections in [release-gates.md](../04-engineering-standards/release-gates.md) from **Partial** to **Evidence attached**, referencing the sanitized production artifacts produced by this checklist. Do not close the gates on local or staging evidence alone.

### Out of scope

- This checklist does **not** add a persisted audit table or migration; audit events remain log/OTL only.
- It does **not** change the tenant auth model, add replicas, or introduce a hosted vendor dependency; the OTLP exporter gate and portable dashboard definitions are preserved.

## Infrastructure-Level Monitoring (Planned, M5)

M5 adds a **separate, lower-level** layer: host and container resource metrics (CPU, memory, disk) via a lightweight agent, private and never publicly exposed. This complements the existing application-level OTLP/Prometheus/Grafana stack — it does not replace it. M5 exists because the current stack observes the *application*, not the *VPS itself*.

## AI cost observability (M27)

A private Admin usage dashboard tracking token consumption and estimated cost per AI-assist capability, with a configurable monthly budget cap.

## Change auditing (M30)

SaveChanges interceptors record change history; surfaced as a readable Admin audit log with a strict, tested exclusion list for any secret-holding field.

## Alerting posture

A small number of high-signal checks (service health, disk usage, failed scheduled jobs, failed releases, AI budget threshold) rather than a large number of low-signal metrics requiring ongoing triage capacity the project does not have.