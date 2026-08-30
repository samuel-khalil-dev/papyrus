# Incident Response Runbook

**Mode:** Diátaxis **How-To + Reference**. Use this procedure for Papyrus
production or staging incidents. It is grounded in the Compose topology: one
PostgreSQL 17 database, `admin-api`, read-only `blog` and `portfolio`, and the
shared telemetry seam.

**Owner:** on-call operator / project owner

**Related:** [`telemetry-runbook.md`](telemetry-runbook.md),
[`alerts.md`](alerts.md), [`deployment.md`](../guides/deployment.md), and
[`release-gates.md`](../architecture/release-gates.md).

## 1. Topology reference

| Component | Role | Operational fact |
| --- | --- | --- |
| `admin-api` | API, auth, media, migration, seed | Sole migrator; serves `/admin`; container port `8080`, published `5205` by default |
| `blog` | Public read-only site | Published `5075`; never migrates |
| `portfolio` | Public read-only site | Published `5024`; never migrates |
| `db` | PostgreSQL 17 | Database `papyrus`; durable `papyrus-postgres-data` volume |
| `papyrus-media` | Admin media storage | Mounted at `/data/media`; must survive Admin restarts |

All hosts expose anonymous `/health` and `/alive` probes. Restrict those probes
to orchestrator and monitoring networks. Request activities are
`Papyrus.Request`; audit records are logger-backed `Papyrus.AuditEvent` events
and EventId `1001`. See the [telemetry runbook](telemetry-runbook.md) for
redaction, sampling, and collector-derived metrics.

## 2. Detection and first response

1. Record detection time, alert name, affected URL/service, and current impact.
2. Check the dashboard and preserve the relevant trace ID, correlation ID, and
   structured log entries. Never copy API keys or credentials into the ticket.
3. Check service state and recent logs:

   ```powershell
   docker compose ps
   docker compose logs --tail 200 admin-api
   docker compose logs --tail 200 db
   ```

4. Probe `/health` and `/alive` from the permitted monitoring network.
5. Classify impact: SEV-1 is an outage or data-loss risk, SEV-2 is material
   degradation such as elevated 5xx or unavailable media, and SEV-3 is a
   non-urgent telemetry or configuration issue.
6. Assign an incident owner and contain before making a risky change.

## 3. Containment and recovery rules

- Remove an unhealthy public host from ingress rotation rather than restarting
  it in a loop.
- Remove `admin-api` from write traffic or block Admin ingress when writes are
  unsafe; public reads are still read-only.
- Do not run migrations or heavy queries while PostgreSQL is under pressure.
- Do not edit or delete an applied migration. Roll back the image only when its
  schema compatibility is known; otherwise create a corrective migration.
- After each recovery action, verify `/health`, `/alive`, request telemetry,
  audit events, and the user-facing path before returning traffic.

## 4. Common incidents

### 4.1 PostgreSQL down or unhealthy

1. Confirm `db` health and inspect only the recent database logs:

   ```powershell
   docker compose ps db
   docker compose logs --tail 200 db
   ```

2. Check host disk and the durable `papyrus-postgres-data` volume through the
   platform. Do not remove the volume.
3. Keep Blog, Portfolio, and Admin write traffic contained while the database
   is unavailable; do not repeatedly restart all web services.
4. If the database container is otherwise recoverable, restart only it:

   ```powershell
   docker compose up -d db
   ```

5. Wait for its health check, then restart `admin-api` and verify its health.
   Blog and Portfolio should be returned after their database-backed pages
   respond normally.
6. If data is damaged, restore the latest verified PostgreSQL backup into the
   approved database recovery target, verify migration history, and document
   the restore before re-applying any needed release migration.

### 4.2 Admin API unhealthy

1. Check `admin-api` logs, `/health`, `/alive`, and the database health state.
2. If the issue is a transient process failure, restart the same immutable image:

   ```powershell
   docker compose up -d admin-api
   ```

3. If the issue followed a release, remove Admin write traffic and redeploy the
   previous immutable image tag after checking database compatibility.
4. Confirm `/admin` and an authenticated, non-destructive API read. Public
   hosts remain read-only, but their database dependency may still require the
   database and Admin startup ordering to be healthy.

### 4.3 Media volume full or unwritable

1. Stop or block uploads at the Admin ingress while preserving public reads.
2. Check capacity and ownership of the durable `papyrus-media` volume; do not
   delete files that are referenced by content.
3. Free or expand capacity according to the storage platform, then restart
   `admin-api` if required:

   ```powershell
   docker compose up -d admin-api
   ```

4. Verify a permitted media operation and persistence across an Admin restart.
   The Admin image pre-creates `/data/media` and assigns it to non-root
   `APP_UID`; do not switch the production container to root.
5. Restore the media volume from its verified backup if files are missing or
   corrupt, then confirm tenant-scoped media URLs.

### 4.4 Migration failure

1. Treat a failed release `migrate` job as a deployment stop. The `deploy` job
   must not run until migration is understood.
2. Capture the migration job output and database migration history. Do not edit
   or delete the applied migration files.
3. Confirm the release used `dotnet-ef` `10.0.11`, the Infrastructure project,
   and the Admin API startup project.
4. Restore from the pre-release backup only if the database change requires it
   and the approved recovery procedure supports it.
5. Correct the model or migration by adding a later corrective migration, then
   rerun one coordinated migration job. Blog and Portfolio never run it.
6. Deploy the web images only after the migration job succeeds and health checks
   pass.

### 4.5 API-key exposure or rotation

1. Treat the key as compromised. Block or restrict Admin ingress if necessary
   and preserve audit events and trace IDs without recording the key itself.
2. For a simple Compose rotation, change the secret-manager value mapped to
   `ADMIN_API_KEY` and recreate `admin-api`:

   ```powershell
   docker compose up -d --force-recreate admin-api
   ```

3. Verify the new key and reject the old key before returning write traffic.
4. For an overlap window, configure the reloadable `AdminApi:ApiKeys` set with
   the new key while retaining the old one, move clients, then remove the old
   key. The legacy single `AdminApi:ApiKey` setting remains a compatibility
   alias; never store keys in the database or source tree.
5. Confirm successful and failed authentication audit events, and check that
   redaction has not emitted credentials. Document the rotation time and
   affected clients.

## 5. Telemetry seam during an incident

1. Query `Papyrus.Request` by service, endpoint, status, and `trace_id`.
2. Query `Papyrus.AuditEvent` or structured EventId `1001` by tenant, action,
   target, outcome, and status.
3. If audit volume drops, follow the ordered checks in
   [Investigate an audit-volume drop](telemetry-runbook.md#5-investigate-an-audit-volume-drop).
   Diagnostic sampling at `0` does not explain missing audit events.
4. Treat `[redacted]` and `[other]` as safety-control outputs, not as permission
   to bypass redaction or cardinality limits.

## 6. Close-out

After stabilization, record the timeline, impact, root cause, containment,
recovery, and corrective actions. Update the runbook or alert mapping if the
incident revealed a missing signal. Record release-gate evidence when the
incident involves deployment, restore, migration ownership, media persistence,
or telemetry operation.

## 7. Release exercise

Before release, exercise a clean Compose deployment, PostgreSQL backup/restore,
media persistence across an Admin restart, and migration ownership. The
recorded 2026-08-29 exercise confirmed PostgreSQL 17 startup, disposable
database restore, sole Admin migration ownership, and media persistence after
the image created `/data/media` with non-root ownership. Temporary drill
artifacts were removed; record subsequent exercises under W6/O7.
