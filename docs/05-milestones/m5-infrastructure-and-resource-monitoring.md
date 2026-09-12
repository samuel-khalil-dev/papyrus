# M5 — Infrastructure & Resource Monitoring

## Objective
Monitor host and container-level resource usage on the VPS, providing the measured data behind ADR-0005's decision to remain a single-VPS modular monolith.

## Background
Nothing currently monitors CPU, memory, or disk usage at the infrastructure level; a full disk in particular would break PostgreSQL without warning.

## Scope

### In scope
- Deploy a lightweight monitoring agent (Netdata or equivalent) covering host and per-container metrics.
- Configure threshold-based alerts (CPU, memory, disk) delivered to the admin via a low-cost channel (webhook/email).
- Apply per-container resource limits in docker-compose to prevent one service starving others.
- Keep the monitoring dashboard private, never publicly exposed.

### Out of scope
- A full observability/tracing platform (Prometheus/Grafana) - explicitly rejected as too heavy for the available VPS headroom.

## Domain changes
- No new aggregates.

## API changes
- No public API changes; monitoring is infrastructure-level.

## UI/Presentation changes
- A private, Admin-only resource dashboard.

## Non-functional requirements
- The monitoring agent itself must have low resource overhead, given the constrained VPS.

## Risks & mitigations
- A heavy monitoring stack could itself become the resource problem it's meant to detect.
- Mitigation: agent choice is explicitly evaluated for low footprint before adoption.

## Definition of Done
- [ ] Host and container metrics are visible in a private dashboard.
- [ ] Threshold alerts fire correctly when tested against simulated high usage.
- [ ] Per-container resource limits are configured and verified not to starve PostgreSQL under load.
- [ ] The dashboard is confirmed unreachable from the public internet.

## Test plan
- Simulated load test verifying alert thresholds fire correctly.
- Verification that the monitoring dashboard is not publicly reachable.
- Verification that per-container limits hold under a synthetic load spike.
