# Release Gates

The following controls define the release baseline and remain mandatory for
subsequent release work. They are enforced by the CI and release workflows
([`ci.yml`](../../.github/workflows/ci.yml),
[`release.yml`](../../.github/workflows/release.yml)) and by the operational
gates recorded below.

## CI pipeline gates

The `verify` job in [`ci.yml`](../../.github/workflows/ci.yml) runs on every
push to `main` and every pull request:

1. **Restore** — `dotnet restore Papyrus.sln`.
2. **Build with warnings treated as errors** —
   `dotnet build Papyrus.sln --configuration Release --no-restore /p:TreatWarningsAsErrors=true`.
3. **Tests** — `dotnet test Papyrus.sln` exercises 84 unique .NET tests in CI:
   70 provider-independent/non-integration tests plus 14 PostgreSQL integration
   tests.
4. **PostgreSQL integration tests** — the `Papyrus.Tests.Integration` filter
   runs the 14 integration tests against a `postgres:17-alpine` service. The
   workflow runs the full suite first and then repeats this integration subset
   explicitly; the unique CI test count remains 84, not 98.
5. **Secret scan** — `gitleaks/gitleaks-action@v2`.
6. **NuGet vulnerability scan** — `dotnet list ... package --vulnerable
   --include-transitive`; fails on any vulnerable package.
7. **OpenAPI drift check** — starts the Admin API, fetches
   `/openapi/v1.json`, and diffs it against the committed
   [`docs/api/openapi.json`](../api/openapi.json).

The `browser-smoke` job (after `verify`) builds the Admin UI, runs the 5
Playwright browser smoke tests against the Admin API, Blog, and Portfolio, and
uploads the report. The `load-smoke` job runs the K6 public and admin load
smokes. The `containers` job builds each image and scans it with Trivy for
`HIGH`/`CRITICAL` vulnerabilities (`exit-code: 1`, `ignore-unfixed: true`).

## Release pipeline gates

The [`release.yml`](../../.github/workflows/release.yml) workflow runs on
`v*.*.*` tags and `workflow_dispatch`:

1. **dependency-scan** — NuGet vulnerability scan and gitleaks secret scan.
2. **images** — builds the admin, blog, and portfolio images, scans each with
   Trivy, generates an SPDX SBOM, and publishes immutable image tags to GHCR.
3. **migrate** — a dedicated migration job in the `production-migrations`
   environment installs `dotnet-ef` 10.0.11 and applies
   `dotnet ef database update` against the production connection string.
4. **deploy** — runs only after `images` and `migrate` succeed, on a
   self-hosted runner in the `production` environment, via
   `docker compose up --detach --remove-orphans`.

## Release-gate evidence

This section records the concrete evidence attached to each open operational
gate. Evidence status is marked accurately; where evidence is genuinely pending
a later sub-task, it is marked **Pending** rather than fabricated.

### W6 — Deployment and operations

**Status: Evidence attached (2026-09-02 UTC local release drill).**

- [`incident-runbook.md`](../runbooks/incident-runbook.md) — incident response
  (detection, triage, containment, recovery, post-incident review) tailored to
  the Papyrus architecture, including a deployment/restore drill checklist.
- [`deployment.md`](../guides/deployment.md) — container build/run, required
  production configuration, migration ownership, and health-endpoint network
  scoping.
- Non-root multi-stage images and health checks are documented in
  [`deployment.md`](../guides/deployment.md) and the Dockerfiles.
- **2026-09-02 drill outcome — PASS (local Compose release candidate):** a
  clean isolated project (`papyrus-release-drill`) built all three images and
  started PostgreSQL 17, Admin, Blog, and Portfolio detached. The database was
  healthy and `/health` returned HTTP 200 on all three web hosts. A custom-format
  `pg_dump` completed at 21,661 bytes; `pg_restore --exit-on-error` restored a
  disposable database, where four EF migration-history rows and seven public
  tables were verified. A sentinel written to `/data/media` remained readable
  after an Admin restart. The migration-ownership test filter passed 2/2 tests;
  the mandated solution build passed with 0 warnings and 0 errors. The
  disposable database, named volumes, stack, sentinel, and backup were removed
  after verification. No credentials or connection strings are recorded.

### W7 — Observability and alerting

**Status: Partial local evidence attached; production collector exercise pending.**

- [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md) — how to operate
  and query the shared `Papyrus` ActivitySource, structured audit events,
  redaction, cardinality, sampling, and audit-volume-drop investigation.
- Telemetry implementation and tests from S30.6/S30.7 (shared `Papyrus` W3C
  ActivitySource, structured audit sink, fail-closed redaction, bounded
  cardinality, configurable sampling, audit retention at 0% sampling).
- **2026-09-02 local outcome — PASS for code/config checks:** the telemetry test
  filter passed 6/6 tests, including fail-closed redaction, W3C activity IDs,
  audit logging without an exporter, and audit survival at 0% diagnostic
  sampling. The dashboard JSON parsed successfully, its panels use the
  documented Prometheus datasource placeholder, and a synthetic W3C
  `traceparent` health request returned HTTP 200. The runbook and alert-family
  mapping were reviewed against the dashboard contract.
- **Pending production verification:** no OTLP collector, Prometheus alert
  evaluator, or Grafana instance is configured in this workspace, so live
  redaction-output inspection, cross-host trace correlation, alert firing and
  recovery, and the audit-volume-drop runbook exercise were not executed.

### W8 — Release documentation and CI

**Status: Partial evidence attached; CI secret-scan artifact pending.**

- [`deployment.md`](../guides/deployment.md) and the CI/release workflows
  ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **2026-09-02 sanitized artifact review — PASS:** the dashboard JSON parsed;
  the committed showcase asset set contains only `admin.png`, `aspire.png`,
  `blog.png`, `health.png`, and `portfolio.png`; and the tracked-file name
  check found no committed `.env`, private-key, or certificate files. Restore
  and release-candidate build checks also passed. No secrets, private URLs, or
  personal data are recorded here.
- **Pending CI verification:** the local `gitleaks` executable is unavailable,
  so sanitized gitleaks output could not be attached. Run the repository's
  `gitleaks/gitleaks-action@v2` in CI (or an approved controlled runner) and
  attach its redacted result to release approval before tagging.

### O4 — Telemetry/audit signal evidence

**Status: Partial local evidence attached; production telemetry configuration pending.**

- [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md) documents the
  telemetry/audit signals and how to investigate an audit-volume drop.
- S30.6/S30.7 telemetry outputs: structured audit events, fail-closed redaction,
  bounded diagnostic cardinality, configurable sampling, and tests proving audit
  logging survives 0% diagnostic sampling.
- **2026-09-02 local outcome — PASS for artifact/config review:** the
  Prometheus-backed dashboard JSON parsed successfully and includes request,
  latency, 5xx, 429, audit, redaction, sampling, cardinality, and app/PostgreSQL
  health views; alert expressions and the telemetry runbook use the same
  collector-derived metric contract.
- **Pending production verification:** this repository contains no deployed
  collector, dashboard service, alert evaluator, or retention policy, so live
  dashboard data, alert delivery/recovery, and retention configuration remain
  unverified and must be attached from the production observability platform.

### O7 — Restore/load drills

**Status: Closed — restore, migration-ownership, media-volume, and load evidence attached (2026-08-29 UTC).**

- Restore/migration/readiness/media-volume drill procedures are documented in
  [`incident-runbook.md`](../runbooks/incident-runbook.md) and
  [`deployment.md`](../guides/deployment.md).
- S30.5 was executed against the Docker Compose PostgreSQL 17 deployment on
  2026-08-29 UTC. The public run recorded HTTP p95 **23.574 ms**, payload p95
  **9,030 bytes**, 0.00% failed requests, and 100.00% checks. The admin run
  recorded HTTP p95 **21.660 ms**, payload p95 **9,256 bytes**, 0.00% failed
  requests, 100.00% checks, and zero 5xx responses.
- Raw K6 summaries: [`s30.5-k6-public-20260829.json`](../performance/s30.5-k6-public-20260829.json)
  and [`s30.5-k6-admin-20260829.json`](../performance/s30.5-k6-admin-20260829.json).
- Restore drill: `pg_dump -Fc` created a 26,130-byte backup from the Compose
  PostgreSQL 17 `papyrus` database; `pg_restore --exit-on-error` restored it into
  a disposable `papyrus_restore` database. Verification found four applied EF
  migration-history rows and seven public tables in the restored database.
- Migration-ownership drill: the solution build passed with 0 warnings/errors;
  the two `MigrationOwnershipTests` passed; source inspection found the sole
  production `MigrateAsync()` call in Infrastructure and the sole Admin startup
  initialization call in `Papyrus.Api.Admin`. No applied migration was edited or
  deleted, and no database update was run as part of the drill.
- Media-volume drill: the initial write exposed that a new named volume was root
  owned and not writable by the non-root Admin container. `Dockerfile.admin` now
  creates `/data/media` and assigns it to `APP_UID`; after rebuilding, a sentinel
  file was written to `/data/media`, the Admin container was restarted, and the
  sentinel remained readable. Inspection confirmed the writable named volume is
  mounted at `/data/media`.
- The combined restore/load gate is closed. Temporary drill credentials, backup
  artifact, restore database, and sentinel file were removed after verification.

### O8 — Performance budgets

**Status: Baseline retained; release re-measurement pending production-like data.**

- [`phase6-baseline.md`](../performance/phase6-baseline.md) records the measured
  baseline: public feed DB p95 below 0.1 ms, four public queries, two admin
  collection queries, and explicit latency/error/payload/upload/allocation/GC
  budgets.
- PostgreSQL query-count budget tests (`PerformanceBudgetTests`) enforce the
  public `≤ 4` and admin `≤ 2` command budgets.
- **2026-09-02 local outcome — NOT RELEASE EVIDENCE:** the local release-candidate
  Compose deployment was verified, but it contains only the small seeded
  dataset and no production-like fixture or capacity environment. The required
  K6/dotnet-counters re-measurement with `Site__PublicBaseUrl` set was therefore
  not claimed as complete. The 2026-08-29 baseline remains informational only;
  repeat the documented workload with production-like data, capture p95,
  errors, payloads, upload cost, allocations, and GC, and attach the sanitized
  artifact before using O8 for release approval.

### O10 — Architecture evidence

**Status: Evidence attached (S30.0 architecture tests).**

- S30.0 architecture tests (NetArchTest) verify modular-monolith boundaries,
  layer rules, and migration ownership.
- [`systemPatterns.md`](../../memory-bank/systemPatterns.md) and the ADRs
  document the modular-monolith decision.
- **Remaining:** record a new ADR only if measured bottlenecks require
  distributed components.
