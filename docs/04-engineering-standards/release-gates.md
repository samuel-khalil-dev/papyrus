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
3. **Tests** — `dotnet test Papyrus.sln` exercises 90 unique .NET tests in CI:
   74 provider-independent/non-integration tests plus 16 PostgreSQL integration
   tests.
4. **PostgreSQL integration tests** — the `Papyrus.Tests.Integration` filter
   runs the 16 integration tests against a `postgres:17-alpine` service. The
   workflow runs the full suite first and then repeats this integration subset
   explicitly; the unique CI test count remains 90, not 106.
5. **Secret scan** — `gitleaks/gitleaks-action@v3.0.0`.
6. **NuGet vulnerability scan** — `dotnet list ... package --vulnerable
   --include-transitive`; fails on any vulnerable package.

The `browser-smoke` job (after `verify`) builds the Admin UI, runs the 5
Playwright browser smoke tests against the Admin API, Blog, and Portfolio, and
uploads the report. The `load-smoke` job runs the K6 public and admin load
smokes. The `containers` job builds each image and scans it with Trivy for
`HIGH`/`CRITICAL` vulnerabilities (`exit-code: 1`, `ignore-unfixed: true`).

OpenAPI drift is verified by the dedicated
[`openapi-schema.yml`](../../.github/workflows/openapi-schema.yml) workflow,
which starts the Admin API, fetches `/openapi/v1.json`, and diffs it against
the committed [`docs/api/openapi.json`](../api/openapi.json).

## Architecture baseline note (2026-09-08)

The `Application → Infrastructure` dependency was inverted to align with
[ADR-0002 (application-owned ports with Infrastructure adapters)](../02-architecture/decisions/adr-0002-application-owned-ports-with-infrastructure-adapters.md).
`Papyrus.Application` owns the persistence ports (`IPublicContentStore`,
`IContentCommandStore`, `IContentQueryStore`, `ITaxonomyCommandStore`,
`IContentLifecycleStore`) and thin facades; `Papyrus.Infrastructure` implements
them with EF Core adapters (`Ef*Store`). The application layer no longer depends
on `PapyrusDbContext`. The test harness registers
`PapyrusSaveChangesInterceptor` so soft-delete and audit behavior is exercised
through the real mechanism. This is an implementation alignment with the
existing ADR and does not change the release-gate evidence recorded below.

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
   `docker compose up --detach --remove-orphans`, then polls the published
   health endpoints (`/health` on the Admin API, `/` on Blog and Portfolio) and
   fails the job if any host does not become reachable.

## Release-gate evidence

This section records the concrete evidence attached to each open operational
gate. Evidence status is marked accurately; where evidence is genuinely pending
a later sub-task, it is marked **Pending** rather than fabricated.

### W6 — Deployment and operations

**Status: Evidence attached (2026-09-02 UTC local release drill).**

- [`runbooks.md`](../06-operations/runbooks.md) — incident response
  (detection, triage, containment, recovery, post-incident review) tailored to
  the Papyrus architecture, including a deployment/restore drill checklist.
- [`deployment-guide.md`](../06-operations/deployment-guide.md) — container
  build/run, required production configuration, migration ownership, and
  health-endpoint network scoping.
- Non-root multi-stage images and health checks are documented in
  [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  Dockerfiles.
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

- [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md)
  — how to operate and query the shared `Papyrus` ActivitySource, structured
  audit events, redaction, cardinality, sampling, the alert families, and the
  audit-volume-drop investigation.
- Telemetry implementation and tests from S30.6/S30.7 (shared `Papyrus` W3C
  ActivitySource, structured audit sink, fail-closed redaction, bounded
  cardinality, configurable sampling, audit retention at 0% sampling).
- **2026-09-02 local outcome — PASS for code/config checks:** the telemetry test
  filter passed 6/6 tests, including fail-closed redaction, W3C activity IDs,
  audit logging without an exporter, and audit survival at 0% diagnostic
  sampling. The dashboard JSON
  ([`observability/dashboards/papyrus-dashboard.json`](../observability/dashboards/papyrus-dashboard.json))
  parsed successfully, its panels use the
  documented Prometheus datasource placeholder, and a synthetic W3C
  `traceparent` health request returned HTTP 200. The runbook and alert-family
  mapping were reviewed against the dashboard contract.
- **Production verification checklist:** the required production observability
  platform (OTLP collector, Prometheus alert evaluator, Grafana, retention
  policy) and the exact verification steps to close this gate are documented in
  the W7/O4 section of
  [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
  No OTLP collector, Prometheus alert evaluator, or Grafana instance is
  configured in this workspace, so live redaction-output inspection, cross-host
  trace correlation, alert firing and recovery, and the audit-volume-drop
  runbook exercise remain pending and must be executed against the production
  deployment using that checklist.

### W8 — Release documentation and CI

**Status: Evidence attached (2026-09-03 UTC gitleaks secret scan).**

- [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  CI/release workflows ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **2026-09-02 sanitized artifact review — PASS:** the dashboard JSON parsed;
  the committed showcase asset set contains only `admin.png`, `aspire.png`,
  `blog.png`, `health.png`, and `portfolio.png`; and the tracked-file name
  check found no committed `.env`, private-key, or certificate files. Restore
  and release-candidate build checks also passed. No secrets, private URLs, or
  personal data are recorded here.
- **2026-09-03 gitleaks secret scan — PASS:** the repository was scanned with
  `gitleaks detect` (SARIF report format, `--redact`) via the official
  `zricethezav/gitleaks` container. The scan covered 8 commits and
  ~23.08 MB, completed with **no leaks found**, and produced a valid SARIF
  report (v2.1.0, 1 run, 0 results). The redacted SARIF artifact is maintained
  locally and is not included in this repository. This satisfies the CI
  secret-scan evidence requirement for release approval.

### O4 — Telemetry/audit signal evidence

**Status: Partial local evidence attached; production telemetry configuration pending.**

- [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md)
  documents the telemetry/audit signals and how to investigate an
  audit-volume drop.
- S30.6/S30.7 telemetry outputs: structured audit events, fail-closed redaction,
  bounded diagnostic cardinality, configurable sampling, and tests proving audit
  logging survives 0% diagnostic sampling.
- **2026-09-02 local outcome — PASS for artifact/config review:** the
  Prometheus-backed dashboard JSON parsed successfully and includes request,
  latency, 5xx, 429, audit, redaction, sampling, cardinality, and app/PostgreSQL
  health views; alert expressions and the telemetry runbook use the same
  collector-derived metric contract.
- **Production verification checklist:** the required production observability
  platform (OTLP collector, dashboard service, alert evaluator, retention
  policy) and the exact verification steps to close this gate are documented in
  the W7/O4 section of
  [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
  This repository contains no deployed collector, dashboard service, alert
  evaluator, or retention policy, so live dashboard data, alert
  delivery/recovery, and retention configuration remain unverified and must be
  attached from the production observability platform using that checklist.

### O7 — Restore/load drills

**Status: Closed — restore, migration-ownership, media-volume, and load evidence attached (2026-08-29 UTC).**

- Restore/migration/readiness/media-volume drill procedures are documented in
  [`runbooks.md`](../06-operations/runbooks.md) and
  [`deployment-guide.md`](../06-operations/deployment-guide.md).
- S30.5 was executed against the Docker Compose PostgreSQL 17 deployment on
  2026-08-29 UTC. The public run recorded HTTP p95 **23.574 ms**, payload p95
  **9,030 bytes**, 0.00% failed requests, and 100.00% checks. The admin run
  recorded HTTP p95 **21.660 ms**, payload p95 **9,256 bytes**, 0.00% failed
  requests, 100.00% checks, and zero 5xx responses.
- Raw K6 summaries (archived): [`s30.5-k6-public-20260829.json`](../history/s30.5-k6-public-20260829.json)
  and [`s30.5-k6-admin-20260829.json`](../history/s30.5-k6-admin-20260829.json).
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

**Status: Evidence attached (2026-09-03 UTC production-like re-measurement).**

- [`performance-budget.md`](performance-budget.md) records the measured
  baseline and explicit latency/error/payload/upload/allocation/GC budgets:
  public feed DB p95 below 0.1 ms, four public queries, two admin collection
  queries, HTTP p95 ≤ 2000 ms (target ≤ 500 ms), public payload ≤ 256 KiB,
  admin payload ≤ 128 KiB, upload ≤ 5 MiB, allocations ≤ 2 MiB/request public
  and ≤ 1 MiB/request admin, and Gen2 collections = 0 during smoke.
- PostgreSQL query-count budget tests (`PerformanceBudgetTests`) enforce the
  public `≤ 4` and admin `≤ 2` command budgets.
- **2026-09-03 production-like re-measurement — PASS:** the documented K6
  workload was re-run against a production-like dataset (8 categories, 12 tags,
  200 projects, 2,000 blog posts, and their tag links) seeded by
  [`seed-production-like.sql`](../../tests/load/seed-production-like.sql), with
  `Site__PublicBaseUrl` set to the production-like origins. The public run
  recorded HTTP p95 **32.79 ms**, payload p95 **16,887 bytes**, 0.00% failed
  requests, and 100.00% checks. The admin run recorded HTTP p95 **19.35 ms**,
  payload p95 **19,827 bytes**, 0.00% failed requests, 100.00% checks, and zero
  5xx responses. All values are within the explicit budgets (p95 ≤ 1000 ms,
  errors < 1%, public payload ≤ 256 KiB, admin payload ≤ 128 KiB).
- **Allocation/GC evidence:** `dotnet-counters` captured the `System.Runtime`
  counters during the admin workload. Total allocated was ~8.95 MB across the
  ~70 s run (~66 KiB/request, well under the ≤ 1 MiB/request admin budget);
  Gen2 collections = 0 during the smoke (matching the budget); total GC pause
  time was 0.117 s; max heap size was ~8.3 MiB.
- Raw artifacts (archived): [`s30.9-k6-public-20260903.json`](../history/s30.9-k6-public-20260903.json),
  [`s30.9-k6-admin-20260903.json`](../history/s30.9-k6-admin-20260903.json),
  and [`s30.9-admin-counters-20260903.json`](../history/s30.9-admin-counters-20260903.json).
  The reproducible workflow is [`perf-baseline.yml`](../../.github/workflows/perf-baseline.yml).

### O9 — Release artifacts and sanitized publication evidence

**Status: Evidence attached (2026-09-03 UTC gitleaks secret scan).**

O9 is a defined release gate (see the actual-progress section of
[`Papyrus_Roadmap.md`](../Papyrus_Roadmap.md)): release artifacts are documented
and sanitized publication evidence is a release gate. Its evidence overlaps with
W8 (release documentation and CI) and is recorded there:

- [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  CI/release workflows ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **2026-09-02 sanitized artifact review — PASS:** the committed showcase asset
  set contains only `admin.png`, `aspire.png`, `blog.png`, `health.png`, and
  `portfolio.png`; the tracked-file name check found no committed `.env`,
  private-key, or certificate files. No secrets, private URLs, or personal data
  are recorded.
- **2026-09-03 gitleaks secret scan — PASS:** the repository was scanned with
  `gitleaks detect` (SARIF report format, `--redact`) via the official
  `zricethezav/gitleaks` container. The scan covered 8 commits and
  ~23.08 MB, completed with **no leaks found**, and produced a valid SARIF
  report (v2.1.0, 1 run, 0 results). The redacted SARIF artifact is maintained
  locally and is not included in this repository. This satisfies the sanitized
  publication secret-scan evidence requirement.

### O10 — Architecture evidence

**Status: Evidence attached (S30.0 architecture tests).**

- S30.0 architecture tests (NetArchTest) verify modular-monolith boundaries,
  layer rules, and migration ownership.
- The modular-monolith decision is recorded in
  [`adr-0001-adopt-a-modular-monolith-over-microservices.md`](../02-architecture/decisions/adr-0001-adopt-a-modular-monolith-over-microservices.md)
  and the boundaries are described in
  [`architecture-overview.md`](../02-architecture/architecture-overview.md).
- **Remaining:** record a new ADR only if measured bottlenecks require
  distributed components.
