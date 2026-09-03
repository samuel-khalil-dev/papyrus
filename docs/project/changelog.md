# Changelog

All notable changes to Papyrus will be documented in this file. The format is
based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); entries
describe repository changes and do not imply that deferred operational gates
have passed.

## [Unreleased]

### Phase 7 — Release readiness and GitHub showcase — 2026-09-03

- **Public documentation (roadmap item 1):** published the architecture
  overview and public overview, threat-model summary, local setup and seed/demo
  flow, API and deployment guides, and known limitations.
- **Showcase assets (roadmap item 2):** added five sanitized screenshots under
  `docs/assets` (blog, portfolio, admin, health, and Aspire) that use the local
  seeded demo content and contain no credentials, private URLs, or personal
  data.
- **Contribution, disclosure, license, and support (roadmap item 3):** verified
  and completed `docs/guides/contributing.md`, `SECURITY.md`, `LICENSE`, and the
  README Support section; confirmed the feature matrix truthfully separates
  implemented, deferred, and release-pending work.
- **Roadmap status reconciliation:** updated the Phase 7 header in
  `docs/project/roadmap.md` from "Not started" to "In progress", recording that
  items 1 and 2 are complete and that item 4 (tagging a release) remains open
  until all release gates and rollback drills pass.
- **Release gate:** a release tag is intentionally not created. Canonical
  production URLs, backup/restore/media-recovery/observability/ingress gates,
  and the fresh-contributor setup gate still require release-time evidence.

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
  enforced public/admin query-count budgets; the S30.10 optimization itself was
  intentionally not applied.
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
- Expanded the verification baseline to 70 passing non-integration .NET tests
  locally. CI adds 14 PostgreSQL-backed integration tests for 84 unique .NET
  tests; the separate Playwright browser suite contains 5 tests. The build has
  zero warnings and zero errors.

### Security and verification — 2026-08-25

- Re-ran the security audit and remediated all W-NEW-01 through W-NEW-05 and
  O-NEW-01 through O-NEW-05 code findings; no critical findings remain.
- Added the `20260825220000_SecuritySearchTrigramIndexes` EF migration for the
  bounded PostgreSQL search path.
- Added security-focused coverage and expanded the local non-integration suite
  to 70 .NET tests. CI runs 14 additional PostgreSQL integration tests (84
  unique .NET tests total), alongside the separate 5-test Playwright suite,
  with a zero-warning, zero-error build.
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
