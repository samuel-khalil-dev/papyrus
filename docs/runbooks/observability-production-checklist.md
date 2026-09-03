# Production Observability Verification Checklist (W7 / O4)

**Mode:** Diátaxis **How-To Guide**. This checklist documents the production
observability platform required to close release gates **W7 (Observability and
alerting)** and **O4 (Telemetry/audit signal evidence)**. It is the operational
counterpart to the local code/config evidence already attached in
[`release-gates.md`](../architecture/release-gates.md).

**Owner:** on-call operator / project owner

**Related:** [`telemetry-runbook.md`](telemetry-runbook.md),
[`alerts.md`](alerts.md), [`incident-runbook.md`](incident-runbook.md),
[`deployment.md`](../guides/deployment.md), and the
[`Papyrus` dashboard](../../ops/dashboards/papyrus-dashboard.json).

---

## 1. Why this checklist exists

The Papyrus telemetry seam (shared W3C `Papyrus` ActivitySource, structured
audit events, fail-closed redaction, bounded cardinality, configurable sampling,
and the OTLP exporter gate) is implemented and locally verified. However, the
release gates W7 and O4 require **live production evidence** that cannot be
produced from this repository alone: a deployed OTLP collector, a Prometheus
alert evaluator, a Grafana instance, and a retention policy. This checklist
defines exactly what must be deployed and verified before those gates can be
closed.

The application emits activities and structured logs; it does **not** expose
Prometheus counters directly. All Prometheus metric names below are
**collector-derived recording views** defined in
[`alerts.md`](alerts.md#collector-boundary-metric-contract). Do not invent
application labels or a second naming scheme.

---

## 2. Required production infrastructure

| Component | Purpose | Required for gate |
| --- | --- | --- |
| OTLP collector (e.g. OpenTelemetry Collector) | Receives `Papyrus.Request` and `Papyrus.AuditEvent` activities from Admin, Blog, and Portfolio via `OTEL_EXPORTER_OTLP_ENDPOINT`; normalizes to Prometheus recording views | W7, O4 |
| Prometheus (or compatible) | Evaluates the alert expressions in [`alerts.md`](alerts.md) | W7 |
| Grafana | Serves the [`papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json) with the Prometheus datasource | W7, O4 |
| Retention policy | Retains audit/telemetry signals per the deployment's compliance window | O4 |

### 2.1 Collector configuration

- Set `OTEL_EXPORTER_OTLP_ENDPOINT` on all three web hosts to the collector's
  OTLP endpoint. An unset endpoint disables OTLP export but **does not** disable
  local structured audit logging (EventId `1001`).
- Configure the collector to record the metric views named in
  [`alerts.md`](alerts.md#collector-boundary-metric-contract):
  `http_server_request_duration_seconds_count`/`_bucket`,
  `papyrus_audit_events_total`, `papyrus_audit_redacted_total`,
  `papyrus_cardinality_collapsed_total`, and `papyrus_diagnostic_sampled_total`.
- Preserve the `papyrus.service`, `papyrus.endpoint`, `http.status_code`,
  `papyrus.audit.outcome`, `papyrus.tenant_id`, `papyrus.audit.action`,
  `papyrus.audit.target`, and `papyrus.audit.status_code` dimensions.

### 2.2 Alert configuration

- Deploy the alert families from [`alerts.md`](alerts.md) to the Prometheus
  evaluator. Every alert must map 1:1 to a telemetry signal; do not add alerts
  without a corresponding signal or runbook.
- The `PapyrusAuditVolumeDrop` alert must never be silenced by a low
  `Telemetry:SamplingRate`, because audit events are not sampled.

### 2.3 Dashboard configuration

- Import [`papyrus-dashboard.json`](../../ops/dashboards/papyrus-dashboard.json)
  into Grafana and bind the `${DS_PROMETHEUS}` datasource placeholder to the
  Prometheus instance.

---

## 3. Verification checklist (close W7 and O4)

Execute each item against the **production** deployment and record the result
(evidence artifact, date, and outcome) under release gate W7/O4 in
[`release-gates.md`](../architecture/release-gates.md). Do not mark an item
complete from local or staging evidence alone.

### 3.1 Redaction-output inspection (W7)

- [ ] Generate a request that triggers an audit event (e.g. an Admin API-key
      mutation) and confirm the emitted activity/log contains **no** API key,
      cookie, request body, or raw credential.
- [ ] Confirm a `[redacted]` value appears for any field outside the allowlist
      and that the redaction is fail-closed (no partial or raw values leak).
- [ ] Record the sanitized redaction output as evidence.

### 3.2 Cross-host trace correlation (W7)

- [ ] Issue a request that flows across hosts (e.g. a public page that
      references Admin media) and confirm the W3C `traceparent`/`tracestate`
      context is preserved across Admin, Blog, and Portfolio.
- [ ] Confirm the trace can be followed end-to-end in the collector/dashboard
      using `trace_id`.

### 3.3 Alert firing and recovery (W7)

- [ ] Trigger each alert family in [`alerts.md`](alerts.md) (or a safe subset)
      and confirm it fires at the configured threshold.
- [ ] Confirm the alert recovers (clears) when the signal returns to normal.
- [ ] Record the alert firing/recovery evidence (e.g. alert history export).

### 3.4 Audit-volume-drop runbook exercise (W7)

- [ ] Execute the [`telemetry-runbook.md`](telemetry-runbook.md#5-investigate-an-audit-volume-drop)
      investigation steps against a simulated or real audit-volume drop.
- [ ] Confirm the `PapyrusAuditVolumeDrop` alert fires and the runbook steps
      identify the root cause.
- [ ] Record the drill outcome.

### 3.5 Live dashboard data (O4)

- [ ] Confirm the Grafana dashboard renders live data for request rate, latency,
      5xx, 429, audit volume, redaction, sampling, cardinality, and
      application/PostgreSQL health.
- [ ] Record a sanitized dashboard screenshot or export as evidence.

### 3.6 Alert delivery/recovery (O4)

- [ ] Confirm alert notifications are delivered to the on-call channel and that
      recovery notifications are sent.
- [ ] Record the delivery evidence.

### 3.7 Retention configuration (O4)

- [ ] Confirm the retention policy is configured for audit/telemetry signals per
      the deployment's compliance window.
- [ ] Record the retention configuration as evidence.

---

## 4. Recording the result

After all checklist items pass, update the W7 and O4 sections in
[`release-gates.md`](../architecture/release-gates.md) from **Partial** to
**Evidence attached**, referencing the sanitized production artifacts produced
by this checklist. Do not close the gates on local or staging evidence alone.

---

## 5. Out of scope

- This checklist does **not** add a persisted audit table or migration; audit
  events remain log/OTL only (see
  [`telemetry-runbook.md`](telemetry-runbook.md#1-emission-model)).
- It does **not** change the tenant auth model, add replicas, or introduce a
  hosted vendor dependency; the OTLP exporter gate and portable dashboard
  definitions are preserved.