# Runbooks

## Incident Response Runbook (Current)

**Owner:** on-call operator / project owner. Ported from `docs/runbooks/incident-runbook.md`. Grounded in the Compose topology: one PostgreSQL 17 database, `admin-api`, read-only `blog` and `portfolio`, and the shared telemetry seam.

### Topology reference

| Component | Role | Operational fact |
| --- | --- | --- |
| `admin-api` | API, auth, media, migration, seed | Sole migrator; serves the SPA at the root; container port `8080`, published `5205` by default |
| `blog` | Public read-only site | Published `5075`; never migrates |
| `portfolio` | Public read-only site | Published `5024`; never migrates |
| `db` | PostgreSQL 17 | Database `papyrus`; durable `papyrus-postgres-data` volume |
| `papyrus-media` | Admin media storage | Mounted at `/data/media`; must survive Admin restarts |

All hosts expose anonymous `/health` and `/alive` probes. Restrict those probes to orchestrator and monitoring networks. Request activities are `Papyrus.Request`; audit records are logger-backed `Papyrus.AuditEvent` events and EventId `1001`. See the [telemetry runbook](../../docs/runbooks/telemetry-runbook.md) for redaction, sampling, and collector-derived metrics.

### Detection and first response

1. Record detection time, alert name, affected URL/service, and current impact.
2. Check the dashboard and preserve the relevant trace ID, correlation ID, and structured log entries. Never copy API keys or credentials into the ticket.
3. Check service state and recent logs:

   ```powershell
   docker compose ps
   docker compose logs --tail 200 admin-api
   docker compose logs --tail 200 db
   ```

4. Probe `/health` and `/alive` from the permitted monitoring network.
5. Classify impact: SEV-1 is an outage or data-loss risk, SEV-2 is material degradation such as elevated 5xx or unavailable media, and SEV-3 is a non-urgent telemetry or configuration issue.
6. Assign an incident owner and contain before making a risky change.

### Containment and recovery rules

- Remove an unhealthy public host from ingress rotation rather than restarting it in a loop.
- Remove `admin-api` from write traffic or block Admin ingress when writes are unsafe; public reads are still read-only.
- Do not run migrations or heavy queries while PostgreSQL is under pressure.
- Do not edit or delete an applied migration. Roll back the image only when its schema compatibility is known; otherwise create a corrective migration.
- After each recovery action, verify `/health`, `/alive`, request telemetry, audit events, and the user-facing path before returning traffic.

### Common incidents

#### PostgreSQL down or unhealthy

1. Confirm `db` health and inspect only the recent database logs:

   ```powershell
   docker compose ps db
   docker compose logs --tail 200 db
   ```

2. Check host disk and the durable `papyrus-postgres-data` volume through the platform. Do not remove the volume.
3. Keep Blog, Portfolio, and Admin write traffic contained while the database is unavailable; do not repeatedly restart all web services.
4. If the database container is otherwise recoverable, restart only it:

   ```powershell
   docker compose up -d db
   ```

5. Wait for its health check, then restart `admin-api` and verify its health. Blog and Portfolio should be returned after their database-backed pages respond normally.
6. If data is damaged, restore the latest verified PostgreSQL backup into the approved database recovery target, verify migration history, and document the restore before re-applying any needed release migration.

#### Admin API unhealthy

1. Check `admin-api` logs, `/health`, `/alive`, and the database health state.
2. If the issue is a transient process failure, restart the same immutable image:

   ```powershell
   docker compose up -d admin-api
   ```

3. If the issue followed a release, remove Admin write traffic and redeploy the previous immutable image tag after checking database compatibility.
4. Confirm the root SPA and an authenticated, non-destructive API read. Public hosts remain read-only, but their database dependency may still require the database and Admin startup ordering to be healthy.

#### Media volume full or unwritable

1. Stop or block uploads at the Admin ingress while preserving public reads.
2. Check capacity and ownership of the durable `papyrus-media` volume; do not delete files that are referenced by content.
3. Free or expand capacity according to the storage platform, then restart `admin-api` if required:

   ```powershell
   docker compose up -d admin-api
   ```

4. Verify a permitted media operation and persistence across an Admin restart. The Admin image pre-creates `/data/media` and assigns it to non-root `APP_UID`; do not switch the production container to root.
5. Restore the media volume from its verified backup if files are missing or corrupt, then confirm tenant-scoped media URLs.

#### Migration failure

1. Treat a failed release `migrate` job as a deployment stop. The `deploy` job must not run until migration is understood.
2. Capture the migration job output and database migration history. Do not edit or delete the applied migration files.
3. Confirm the release used `dotnet-ef` `10.0.11`, the Infrastructure project, and the Admin API startup project.
4. Restore from the pre-release backup only if the database change requires it and the approved recovery procedure supports it.
5. Correct the model or migration by adding a later corrective migration, then rerun one coordinated migration job. Blog and Portfolio never run it.
6. Deploy the web images only after the migration job succeeds and health checks pass.

#### API-key exposure or rotation

1. Treat the key as compromised. Block or restrict Admin ingress if necessary and preserve audit events and trace IDs without recording the key itself.
2. For a simple Compose rotation, change the secret-manager value mapped to `ADMIN_API_KEY` and recreate `admin-api`:

   ```powershell
   docker compose up -d --force-recreate admin-api
   ```

3. Verify the new key and reject the old key before returning write traffic.
4. For an overlap window, configure the reloadable `AdminApi:ApiKeys` set with the new key while retaining the old one, move clients, then remove the old key. The legacy single `AdminApi:ApiKey` setting remains a compatibility alias; never store keys in the database or source tree.
5. Confirm successful and failed authentication audit events, and check that redaction has not emitted credentials. Document the rotation time and affected clients.

### Telemetry seam during an incident

1. Query `Papyrus.Request` by service, endpoint, status, and `trace_id`.
2. Query `Papyrus.AuditEvent` or structured EventId `1001` by tenant, action, target, outcome, and status.
3. If audit volume drops, follow the ordered checks in the [audit-volume-drop investigation](../../docs/runbooks/telemetry-runbook.md). Diagnostic sampling at `0` does not explain missing audit events.
4. Treat `[redacted]` and `[other]` as safety-control outputs, not as permission to bypass redaction or cardinality limits.

### Close-out

After stabilization, record the timeline, impact, root cause, containment, recovery, and corrective actions. Update the runbook or alert mapping if the incident revealed a missing signal. Record release-gate evidence when the incident involves deployment, restore, migration ownership, media persistence, or telemetry operation.

### Release exercise (drill-record convention)

Before release, exercise a clean Compose deployment, PostgreSQL backup/restore, media persistence across an Admin restart, and migration ownership. The recorded 2026-08-29 exercise confirmed PostgreSQL 17 startup, disposable database restore, sole Admin migration ownership, and media persistence after the image created `/data/media` with non-root ownership. Temporary drill artifacts were removed; record subsequent exercises under W6/O7.

---

## Failed deployment / failed health check
1. Confirm the failure is genuine.
2. Halt promotion; roll back to the previous pinned image.
3. Investigate via CI and health-check logs before retrying.

## Backup restore drill
1. Provision a non-production target.
2. Restore the most recent backup and verify data integrity.
3. Document the outcome; repeat periodically, not only once.

## Infrastructure resource alert (Planned, M5)
1. Identify the resource (CPU/memory/disk) and the responsible container via the monitoring dashboard.
2. For disk-usage alerts specifically, check backup retention and media growth first - the most likely causes.
3. Scale down or clean up the responsible workload; escalate to a VPS upgrade only if the trend, not a one-time spike, justifies it, per ADR-0005.

## AI provider failure or budget cap reached (Planned, M25/M27)
1. Confirm whether the active provider profile failed its connection or the budget cap triggered the graceful disable.
2. For a provider failure, use the Admin's pre-configured fallback profile if one exists, after a test-connection pass.
3. For a budget cap, review usage by capability before simply raising the cap - identify what drove the spike.

## Scheduled-publish failure (Planned, M17)
1. Check the job's execution log.
2. Manually publish via Admin as an immediate override.
3. Fix the underlying failure before relying on scheduling again.

## Contact form or comment abuse (Planned, M7, M16)
1. Confirm Cloudflare Turnstile (M13) is active on the protected forms and correctly configured.
2. Review submission volume against rate limits; tighten if abuse is confirmed.

## GitHub API unavailability (Planned, M9, M22)
1. Confirm affected widgets degrade gracefully.
2. No emergency action needed if the fallback functions correctly.
