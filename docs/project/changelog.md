# Changelog

All notable changes to Papyrus will be documented in this file.

## [Unreleased]

### Phase 6 — Observability, test depth, and performance budgets — 2026-08-29

- **S30.0 architecture tests:** added NetArchTest coverage for modular-monolith
  boundaries, layer rules, and migration ownership.
- **S30.1/S30.2 test depth:** expanded the testing pyramid with handler,
  PostgreSQL integration, HTTP security/contract, and migration-drift coverage.
- **S30.3 handler and migration-drift tests:** added application-handler and
  migration-drift verification.
- **S30.4 PostgreSQL/integration tests:** added provider-behavior, index,
  transaction, concurrency, and filter coverage.
- **S30.6 telemetry core:** added the shared `Papyrus` W3C ActivitySource,
  request middleware, structured `IAuditEventSink`, and telemetry options.
- **S30.7 tenant-safe telemetry:** added fail-closed `Redactor`, bounded
  `CardinalityGuard`, and `TelemetrySamplingPolicy`; audit events survive 0%
  diagnostic sampling.
- **S30.8 dashboard:** added the portable Grafana dashboard
  (`ops/dashboards/papyrus-dashboard.json`) covering request rate/latency/error,
  rate-limit responses, audit events, redaction, sampling, cardinality, and
  app/PostgreSQL health.
- **S30.9/S30.10 performance budgets:** recorded the measured baseline and
  enforced public/admin query-count budgets.
- **S30.11 runbooks:** added the alert, incident, and telemetry runbooks
  (`docs/runbooks/alerts.md`, `docs/runbooks/incident-runbook.md`,
  `docs/runbooks/telemetry-runbook.md`).
- **S30.5 load evidence:** captured and committed K6 load evidence
  (`docs/performance/s30.5-k6-public-20260829.json`,
  `docs/performance/s30.5-k6-admin-20260829.json`). Public run HTTP p95
  23.574 ms, payload p95 9,030 bytes, 0% failed, 100% checks; admin run HTTP
  p95 21.660 ms, payload p95 9,256 bytes, 0% failed, 100% checks, 0 5xx.
- **S30.12 RLS decision ADR:** added
  `docs/architecture/decisions/0007-postgres-rls-decision.md`; decision DECLINE
  Postgres RLS, with application-level tenant filtering remaining the sole
  enforcement layer.
- Expanded the full suite to 73 .NET tests plus 5 Playwright browser tests with
  a zero-warning, zero-error build.

### Security and verification — 2026-08-25

- Re-ran the security audit and remediated all W-NEW-01 through W-NEW-05 and
  O-NEW-01 through O-NEW-05 code findings; no critical findings remain.
- Added the `20260825220000_SecuritySearchTrigramIndexes` EF migration for the
  bounded PostgreSQL search path.
- Added security-focused coverage and expanded the full suite to 73 .NET tests
  plus 5 Playwright browser tests with a zero-warning, zero-error build.
- Hardened URL generation, compatibility-route pagination, feed/sitemap/media
  cancellation, process-local store eviction, CSP/theme loading, health
  responses, and deployment/network release-gate documentation.

## [0.5.0] - 2026-08-25

- **Containers and CI/CD:** Added separate multi-stage images for Admin, Blog,
  and Portfolio, Docker Compose orchestration, health checks, non-root runtime
  execution, image scanning, SBOM generation, and a release migration job.
- **REST contracts:** Added shared request/response DTOs, versioned `/api/v1`
  routes, pagination, validation problem details, OpenAPI metadata, and schema
  drift checks.
- **Persistence:** Added application-layer projections and commands, tenant-safe
  EF Core persistence, audited lifecycle fields, concurrency tokens, indexes,
  constraints, and the Phase 3 migration.
- **Security hardening:** Added opaque server-side admin sessions with expiry,
  rotation, revocation, login throttling, antiforgery protection, tenant
  isolation, safe markdown/media handling, and security headers.
