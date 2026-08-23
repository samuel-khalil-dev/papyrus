# Changelog

All notable changes to Papyrus will be documented in this file.

## [Unreleased]

- Phase 6 planning: observability, test depth, and performance budgets are next.

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
