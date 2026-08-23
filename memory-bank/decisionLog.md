# Decision Log

## 2026-08-29 — S30.0 architecture test tooling

- Added `NetArchTest.Rules` version 1.3.2 to the existing solution test project,
  rather than creating a second test assembly, because `Papyrus.Tests` is the
  solution-wired test project and the canonical Phase 6 plan targets its
  `Architecture/` directory.
- Migration ownership is tested structurally: the repository must contain one
  non-migration `.MigrateAsync()` call in Infrastructure and one
  `InitializeDatabaseAsync()` startup call in the Admin delivery layer.

## 2026-08-29 — S30.1 PostgreSQL integration harness

- Kept the existing `Papyrus.Tests` assembly and added an `Integration/` folder,
  matching the plan's target paths and preserving the solution's single test
  project.
- The fixture uses PostgreSQL 17 with the existing EF migrations, a dedicated
  `papyrus_test` database, and a known `integration` tenant. A
  `PAPYRUS_TEST_CONNECTION_STRING` environment variable provides a CI/local
  override without checked-in secrets.
- Added a Compose `test-db` profile and a GitHub Actions `postgres:17-alpine`
  service container. The harness has no InMemory fallback.

## 2026-08-29 — S30.6 telemetry core

- Chose logger/OTLP-backed audit events instead of a persisted audit table, so
  no schema migration is required. The logger sink is active by default and is
  independent of OTLP exporter/listener availability; audit events remain
  locally observable when diagnostic export is unavailable.
- Centralized custom instrumentation on the single `Papyrus` ActivitySource and
  forced W3C IDs for modular-monolith trace propagation. Request middleware uses
  a stable activity name and bounded service/endpoint tags; S30.7 owns the next
  redaction/cardinality policy layer.
- Replaced Admin's free-form audit messages with structured events. API-key
  authentication emits success/failure events, while Admin mutation/error
  requests emit tenant, actor, action, target, correlation, outcome, status, and
  trace metadata without credentials.

## 2026-08-29 — S30.7 telemetry safety controls

- Redaction is applied inside `StructuredAuditEventSink`, before either logger
  state or audit activity tags are created. The allowlist preserves only valid
  tenant identifiers, fixed identities/actions/outcomes, bounded route-shaped
  targets, and bounded correlation/trace identifiers; all other values fail
  closed to `[redacted]`.
- Diagnostic request activities use `Telemetry__SamplingRate`; audit events do
  not consult that policy and remain logger-backed at 0% diagnostic volume.
  `Telemetry__CardinalityLimit` bounds unique service/endpoint tag values and
  maps overflow to `[other]`. No migration or external collector dependency was
  added.

## 2026-08-29 — S30.3 handler and migration-drift tests

- Kept handler tests provider-independent with the existing isolated InMemory
  `TestDb` helper; PostgreSQL-specific behavior remains covered by the S30.1
  integration collection.
- Implemented migration drift protection as an EF model gate using
  `Database.HasPendingModelChanges()`, supplemented by a PostgreSQL
  `GenerateCreateScript()`/migration-snapshot consistency test. The test uses
  the existing five-migration assembly and does not connect or alter schema.
- No migration was authored, rewritten, or deleted.

## 2026-08-29 — S30.2 PostgreSQL depth and S30.4 browser smoke

- Kept the S30.2 tests in the existing `Papyrus.Tests` integration collection,
  using the S30.1 fixture and real migrations. PostgreSQL-only assertions cover
  `pg_trgm`/GIN plans, partial indexes, transaction isolation, duplicate-key
  concurrency, filters, and projected no-tracking query shape without schema
  changes.
- Selected the plan-locked Playwright 1.55.0 runner under `tests/e2e`, added
  `@types/node` for strict environment typing, and retained the CI browser job
  as non-required smoke coverage. The admin UI test authenticates through the
  actual local-storage API-key seam; there is no legacy browser login flow.
- Removed redundant explicit content-handler transactions because EF Core's
  single `SaveChangesAsync` is atomic and remains compatible with the configured
  Npgsql retry execution strategy. No migration was authored or modified.

## 2026-08-29 — S30.5 focused K6 load tests

- Selected the plan-locked Grafana K6 scripts under `tests/load/k6`; public
  paginated Blog/Portfolio feed and search receives the primary concurrency
  budget, while admin load remains bounded and uses the single process-local
  API key and `main` tenant.
- Expected admin 429 responses are recorded rather than treated as server
  failures because the fixed-window limiter is per source IP. Any 5xx response
  fails the admin threshold. The scripts create unpublished temporary posts and
  therefore target disposable/reset Postgres data.
- Added a short CI `load-smoke` job with PostgreSQL 17 and
  `grafana/setup-k6-action@v1`; no soak, replica, auth-model, production URL,
  migration, or production-code change was introduced.

## 2026-08-29 — S30.9/S30.10 performance baseline and optimization gate

- Added a dispatch-only performance workflow instead of making a load soak part
  of every PR. It starts the existing three web hosts against PostgreSQL 17 and
  archives K6 summary JSON for latency, errors, checks, and response payload
  bytes; production `Site__PublicBaseUrl` remains a deployment-time setting.
- Added an EF `DbCommandInterceptor` budget test. Public feed is capped at four
  commands (count, page, featured, tags); versioned admin collection is capped at
  count plus page (two commands). This is a test-only measurement seam and adds
  no schema or provider.
- Retained the current projected/no-tracking/async/bounded query shape. Captured
  PostgreSQL p95 below 0.1 ms and no N+1/sequential scan evidence do not justify
  an S30.10 production change. No speculative allocation optimization, migration,
  service extraction, replica, or external dependency was added.

## 2026-08-29 — S30.8 dashboards and alert families

- Chose a portable Grafana dashboard using the `${DS_PROMETHEUS}` datasource
  placeholder and normalized OpenTelemetry span metrics for the existing
  `Papyrus.Request` and `Papyrus.AuditEvent` activities.
- Kept alert mappings grounded in existing activity names, status/service/
  endpoint attributes, redaction output, cardinality `[other]` output, and
  platform health probes. Redaction/cardinality counters are documented as
  collector-derived views rather than application instrumentation.
- Added `PapyrusAuditVolumeDrop` as a 15-minute audit-activity comparison with
  the preceding hour and a minimum-volume guard, because audit events are
  logger-backed and unaffected by diagnostic sampling.

## 2026-08-29 — S30.11 runbooks and release-gate evidence

- Added incident and telemetry runbooks and corrected the documentation baseline
  to 73 .NET tests plus 5 Playwright browser tests.
- Attached release-gate evidence for W6, W7, W8, O4, O8, and O10. O7 is
  explicitly pending S30.5 K6 load-test evidence; optional S30.12 remains the
  Postgres RLS decision ADR.
