# Incident Response Runbook

**Scope:** Papyrus production and staging incidents. This runbook is tailored to
the Papyrus architecture: an Aspire-orchestrated .NET 10 modular monolith with an
admin API/UI, public blog, public portfolio, a single PostgreSQL 17 database, and
OpenTelemetry-based telemetry. It is a release-gate artifact for **W6** (deployment
and operations) and **W7** (observability and alerting).

**Owner:** on-call operator / project owner
**Related documents:** [`telemetry-runbook.md`](telemetry-runbook.md),
[`deployment.md`](../guides/deployment.md), [`audit-register.md`](../security/audit-register.md)

---

## 1. Architecture at a glance

| Component | Role | Notes |
| --- | --- | --- |
| `admin-api` | Writes, auth, media, migrations, seed | Sole migration owner; API-key authenticated |
| `admin-ui` | React SPA served at `/admin` | Dev-only Aspire npm resource; proxied `/api/v1` |
| `blog` | Public read-only host | Waits on admin-api health; never migrates |
| `portfolio` | Public read-only host | Waits on admin-api health; never migrates |
| PostgreSQL 17 | Single database | Durable volume; `papyrus` database |
| Telemetry | OpenTelemetry via shared `Papyrus` ActivitySource | Structured audit events, redaction, sampling |

All three web hosts share the same request telemetry seam. The admin host owns
migrations and seed; public hosts are read-only. Health endpoints `/health` and
`/alive` are anonymous but must be network-restricted to orchestrator/monitoring
networks.

---

## 2. Detection

Signals come from the telemetry pipeline (see
[`telemetry-runbook.md`](telemetry-runbook.md)) and platform health checks.

### 2.1 Alert sources

- **Health checks:** `/health` and `/alive` on each container; the orchestrator
  probes these. A failing probe indicates a container or dependency problem.
- **HTTP error rate:** K6/load and request telemetry thresholds (`<1%` errors;
  admin `5xx = 0`). See [`phase6-baseline.md`](../performance/phase6-baseline.md).
- **Latency:** public/admin HTTP p95 budget `≤ 1000 ms` (target `≤ 500 ms`).
- **Audit volume drop:** a sudden drop in structured audit events can indicate a
  telemetry outage or a redaction/sampling misconfiguration. Investigate per
  [`telemetry-runbook.md`](telemetry-runbook.md#investigating-an-audit-volume-drop).
- **Database:** PostgreSQL connection failures, slow queries, or disk/volume
  pressure on the durable media and database volumes.
- **Migration failures:** the release `migrate` job failing before `deploy`.

### 2.2 Severity classification

| Severity | Definition | Example |
| --- | --- | --- |
| **SEV-1** | Public outage or data loss; admin write path unavailable | Blog/portfolio down; database unreachable |
| **SEV-2** | Degraded service; admin API errors; media unavailable | Elevated 5xx, high latency, media volume full |
| **SEV-3** | Non-urgent operational issue | Telemetry gap, stale config, non-blocking alert |

---

## 3. Triage

1. **Confirm the signal.** Check the telemetry dashboard and container logs
   (`docker compose logs <service>`). Do not act on a single flapping probe.
2. **Identify the affected component.** Is it `admin-api`, `blog`, `portfolio`,
   PostgreSQL, or the ingress/proxy?
3. **Check recent changes.** Review the last release, migration, or configuration
   change. A migration or config change is a common root cause.
4. **Classify severity** (SEV-1/2/3) and assign an incident owner.
5. **Open a tracking issue** with the detection time, affected component, and
   current impact.

---

## 4. Containment

Containment limits blast radius before recovery. Choose the least disruptive
action that stops the bleeding.

- **Public site degraded:** If `blog` or `portfolio` is unhealthy, remove it from
  the load balancer/ingress rotation rather than restarting in a loop.
- **Admin write path failing:** Stop admin writes by removing `admin-api` from
  rotation or blocking the admin ingress. Public reads are unaffected because
  public hosts are read-only and independent.
- **Database pressure:** If PostgreSQL is the bottleneck, do **not** run
  migrations or heavy queries. Pause non-essential load and consider scaling the
  database volume/instance.
- **Suspected compromise:** If an API key or credential is suspected exposed,
  rotate `AdminApi__ApiKey` immediately (see
  [`deployment.md`](../guides/deployment.md)) and revoke the affected key.
- **Media volume full:** Stop uploads (admin write path) and free space on the
  durable media volume before recovery.

---

## 5. Recovery

Recovery restores service to a known-good state. Prefer the least invasive step
first.

1. **Restart the affected container** with the same immutable image tag:
   `docker compose up -d <service>`. Public hosts wait on admin-api health.
2. **Verify health** on `/health` and `/alive` before returning traffic.
3. **Roll back a bad release:** redeploy the previous immutable image tag. Do not
   edit or delete applied migrations; if a migration is the problem, generate a
   corrective migration rather than editing history (see
   [`deployment.md`](../guides/deployment.md)).
4. **Database recovery:** restore from the latest verified PostgreSQL backup,
   then re-apply the migration history up to the release point. Verify the
   migration history before and after restore.
5. **Media recovery:** restore the durable media volume from backup if media is
   missing or corrupt.
6. **Confirm telemetry:** verify audit events and request traces are flowing again
   before declaring recovery complete.

---

## 6. Post-incident review

After the service is stable:

1. **Document the timeline:** detection, triage, containment, recovery, and
   resolution times.
2. **Identify root cause** and contributing factors.
3. **Define corrective actions** (code, config, runbook, or process changes) and
   assign owners.
4. **Update the runbooks** with any new detection signal or recovery step learned.
5. **Record the incident** in the changelog and, if it affects a release gate,
   update [`audit-register.md`](../security/audit-register.md) evidence.

---

## 7. Escalation and contacts

- **On-call operator:** first responder for SEV-2/SEV-3.
- **Project owner:** SEV-1 decision authority and release-gate owner.
- **Database administrator:** PostgreSQL restore and volume recovery.
- **Security contact:** for suspected compromise or credential exposure.

---

## 8. Runbook exercise (release gate W6)

Before release, execute a deployment/restore drill that covers:

- Clean deployment of the three images and PostgreSQL.
- A database backup and restore cycle.
- Media persistence across a container restart.
- A migration-ownership drill confirming only `admin-api` applies migrations.

Record the drill outcome as **W6** evidence in
[`release-gates.md`](../architecture/release-gates.md).