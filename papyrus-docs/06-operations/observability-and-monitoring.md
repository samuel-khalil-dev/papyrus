# Observability & Monitoring

## Application-Level Observability (Current)

Papyrus runs a real OTLP-based telemetry pipeline today: an OTLP collector feeding Prometheus and Grafana, with a dedicated `Papyrus` `ActivitySource`, controlled sampling (`Telemetry:SamplingRate`), a cardinality limit (`Telemetry:CardinalityLimit`), and nine live alert families defined in `docs/runbooks/alerts.md`. `EventId 1001` is intentionally never sampled. The Grafana dashboard definition lives at `ops/dashboards/papyrus-dashboard.json`.

Do not describe this stack as absent, and do not propose replacing it — Prometheus and Grafana are not "too heavy" for this platform; they are already running successfully on it.

## Infrastructure-Level Monitoring (Planned, M5)

M5 adds a **separate, lower-level** layer: host and container resource metrics (CPU, memory, disk) via a lightweight agent, private and never publicly exposed. This complements the existing application-level OTLP/Prometheus/Grafana stack — it does not replace it. M5 exists because the current stack observes the *application*, not the *VPS itself*.

## AI cost observability (M27)

A private Admin usage dashboard tracking token consumption and estimated cost per AI-assist capability, with a configurable monthly budget cap.

## Change auditing (M30)

SaveChanges interceptors record change history; surfaced as a readable Admin audit log with a strict, tested exclusion list for any secret-holding field.

## Alerting posture

A small number of high-signal checks (service health, disk usage, failed scheduled jobs, failed releases, AI budget threshold) rather than a large number of low-signal metrics requiring ongoing triage capacity the project does not have.