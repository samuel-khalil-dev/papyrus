# Changelog

**Current:** this file records the release and phase history of the implemented system (the legacy `docs/project/` changelog was retired during the documentation consolidation).

All notable changes to Papyrus will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); entries describe repository changes and do not imply that deferred operational gates have passed.

## [Unreleased]

### Documentation currency and agent-guidance ownership — 2026-09-27

- **Single source of truth:** `docs/07-governance/documentation-maintenance.md` now states that `docs/` is the only authoritative record of project facts, that a fact lives in exactly one place, and that session or workstation-specific context belongs to agent tooling rather than to a second documentation tree.
- **Retired the untracked memory bank:** deleted `memory-bank/` (seven files). It was untracked by the 2026-09-08 "stop tracking internal memory-bank" change, which meant it could not be diffed, reviewed, or checked by CI. It drifted to the point of contradicting the tracked record — it still described the M0–M32 milestone sequence as unstarted and carried superseded package versions — and it duplicated the ADR index, the roadmap's two-schema model, the project charter, and the security baseline. This reverses the scope of that earlier change: the files are removed rather than merely untracked.
- **`AGENTS.md` is now tracked.** It was gitignored, so the primary agent contract was absent from every fresh clone and every git worktree and exempt from review. It is reviewed on the same basis as any other document, per the documentation-maintenance review cadence.
- **Ignore-file cleanup:** removed the `memory-bank/` entries from `.gitignore` and `.dockerignore`, and removed the vestigial `docs/security/`, `docs/architecture/`, and `docs/review/` rules, which were left over from the pre-consolidation layout and matched nothing.
- **Milestone currency:** `05-milestones/m0-architectural-guardrails.md` now records the delivered endpoint grouping, restates the background in present tense, and records the concrete finding behind each of the three open items.
- **Status surfaces refreshed:** `README.md` (documentation home) states current milestone and release-gate status; `02-architecture/architecture-overview.md` flags the Presentation-to-Infrastructure boundary as under audit; `00-overview/feature-matrix.md` gains an architecture-guardrails row that separates the implemented tests from the boundary that is not yet enforced.
- **Operational notes relocated:** the declared-but-unused Admin UI dependencies moved to `06-operations/vendor-dependency-map.md`, and the k6 shared-runner latency note to `04-engineering-standards/performance-budget.md`.
- The `openapi-typescript` exact-version pin is now recorded as the current state, superseding the 2026-09-03 observation that it was declared as a caret range.

### M0 endpoint grouping, roadmap schema reconciliation, and ADR index — 2026-09-27

- **Admin endpoint grouping (M0 Definition-of-Done item 1):** split the 403-line `AdminApiEndpoints.cs` into `src/Papyrus.Api.Admin/Endpoints/` (taxonomy, projects, posts, media, plus shared `AdminEndpointSupport`). A verbatim lift by design: route templates, handler bodies, response metadata, registration order, and the 23 versioned endpoint registrations are unchanged, so `docs/api/openapi.json` does not drift.
- **Roadmap two-schema model:** the roadmap tracked delivery through Phases 0–7 and Milestones M0–M32 without stating how they relate, which read as a contradiction. Section 3 now opens with an explicit relationship, the overlap section names M5, M6, and M8 and is written against repository evidence, and M0 is restated as one of four DoD items met.
- **ADR index:** `02-architecture/decisions/README.md` indexes ADR-0001 through ADR-0009 with one-line summaries, records which decisions are cited where, and states how to supersede a decision rather than edit one. ADR-0002, ADR-0003, and ADR-0006 previously had no index entry.
- **Citation correction:** `02-architecture/c4-diagrams.md` cited ADR-0007 for the RLS decision; the RLS evaluation and decline are recorded in ADR-0004. `04-engineering-standards/release-gates.md` attributed the port/adapter inversion to ADR-0001; it is ADR-0002.

### Repository hygiene and dependency automation — 2026-09-14 to 2026-09-23

- **Repository hygiene, phases 1–3:** pin refresh, documentation alignment, and metadata file corrections.
- **Dependabot:** weekly schedule with `open-pull-requests-limit: 10` across NuGet, npm (Admin UI and e2e), and GitHub Actions; Node image major bumps blocked; `@types/node` pinned to the Node 24 line; Dependabot configured after an initial round of failures from an unsupported `versioning-strategy` key and a missing `pull-requests: read` permission for the gitleaks pull-request scan.
- **Dependency updates:** Markdig 1.3.2 to 1.4.0, the OpenTelemetry group, the Aspire group, `dotnet-microsoft` across the API and test projects, the Admin UI non-major group, GitHub Actions, `@types/node` in the e2e project, and three container base images.

### Documentation consolidation into `docs/` — 2026-09-11 to 2026-09-12

- Reconciled the external `papyrus-docs` package against the implemented state, then consolidated it into `docs/`, retired the legacy documentation tree, and executed the final cleanup pass. The portable Grafana dashboard is now tracked in-repo under `docs/observability/dashboards/`.

### Application/Infrastructure port-adapter alignment — 2026-09-08

- Inverted the `Application → Infrastructure` dependency to match ADR-0002. `Papyrus.Application` now owns the persistence ports (`IPublicContentStore`, `IContentCommandStore`, `IContentQueryStore`, `ITaxonomyCommandStore`, `IContentLifecycleStore`) and thin facades; `Papyrus.Infrastructure` implements them as EF Core adapters (`Ef*Store`) and holds the EF-specific search predicates and projections. The application layer no longer depends on `PapyrusDbContext`.

### CI/CD hardening and self-hosted deployment — 2026-09-08 to 2026-09-09

- Pinned third-party actions to immutable commit SHAs and official actions to current majors, granted `packages: write` for GHCR publishing, made the OpenAPI comparison CRLF-aware, pinned k6 for reproducible load baselines, added a release dry-run guard, lowercased the GHCR image prefix, and moved release deployment to a self-hosted runner with GHCR pull credentials. Added a `js-yaml` high-severity override and a `fetch-depth: 0` checkout so the gitleaks diff range resolves in a shallow clone.

### Phase 7 — Release readiness and GitHub showcase — 2026-09-03

- **Public documentation (roadmap item 1):** published the architecture overview and public overview, threat-model summary, local setup and seed/demo flow, API and deployment guides, and known limitations.
- **Showcase assets (roadmap item 2):** added five sanitized screenshots under `docs/assets` (blog, portfolio, admin, health, and Aspire) that use the local seeded demo content and contain no credentials, private URLs, or personal data.
- **Contribution, disclosure, license, and support (roadmap item 3):** verified and completed `docs/07-governance/contribution-workflow.md`, `SECURITY.md`, `LICENSE`, and the README Support section; confirmed the feature matrix truthfully separates implemented, deferred, and release-pending work.
- **Roadmap status reconciliation:** updated the Phase 7 header in `docs/Papyrus_Roadmap.md` from "Not started" to "In progress", recording that items 1 and 2 are complete and that item 4 (tagging a release) remains open until all release gates and rollback drills pass.
- **Release gate:** a release tag is intentionally not created. Canonical production URLs, backup/restore/media-recovery/observability/ingress gates, and the fresh-contributor setup gate still require release-time evidence.

### Phase 6 — Observability, test depth, and performance budgets — 2026-08-29

- **S30.0 architecture tests:** added NetArchTest coverage for modular-monolith boundaries, layer rules, and migration ownership.
- **S30.1/S30.2 test depth:** expanded the testing pyramid with handler, PostgreSQL integration, HTTP security/contract, and migration-drift coverage.
- **S30.3 handler and migration-drift tests:** added application-handler and migration-drift verification.
- **S30.4 PostgreSQL/integration tests:** added provider-behavior, index, transaction, concurrency, and filter coverage.
- **S30.6 telemetry core:** added the shared `Papyrus` W3C ActivitySource, request middleware, structured `IAuditEventSink`, and telemetry options.
- **S30.7 tenant-safe telemetry:** added fail-closed `Redactor`, bounded `CardinalityGuard`, and `TelemetrySamplingPolicy`; audit events survive 0% diagnostic sampling.
- **S30.8 dashboard:** added the portable Grafana dashboard (maintained locally, outside the repository) covering request rate/latency/error, rate-limit responses, audit events, redaction, sampling, cardinality, and app/PostgreSQL health.
- **S30.9/S30.10 performance budgets:** recorded the measured baseline and enforced public/admin query-count budgets; the S30.10 optimization itself was intentionally not applied.
- **S30.11 runbooks:** added the alert, incident, and telemetry runbooks (now consolidated under `docs/06-operations/runbooks.md`).
- **S30.5 load evidence:** captured and committed K6 load evidence (`docs/history/s30.5-k6-public-20260829.json`, `docs/history/s30.5-k6-admin-20260829.json`). Public run HTTP p95 23.574 ms, payload p95 9,030 bytes, 0% failed, 100% checks; admin run HTTP p95 21.660 ms, payload p95 9,256 bytes, 0% failed, 100% checks, 0 5xx.
- **S30.12 RLS decision ADR:** decision DECLINE Postgres RLS, with application-level tenant filtering remaining the sole enforcement layer. Recorded in `docs/02-architecture/decisions/adr-0004-retain-multitenant-entities.md`.
- Expanded the verification baseline to 70 passing non-integration .NET tests locally. CI adds 16 PostgreSQL-backed integration tests for 86 unique .NET tests; the separate Playwright browser suite contains 5 tests. The build has zero warnings and zero errors.

### Security and verification — 2026-08-25

- Re-ran the security audit and remediated all W-NEW-01 through W-NEW-05 and O-NEW-01 through O-NEW-05 code findings; no critical findings remain.
- Added the `20260825220000_SecuritySearchTrigramIndexes` EF migration for the bounded PostgreSQL search path.
- Added security-focused coverage and expanded the local non-integration suite to 70 .NET tests. CI runs 16 additional PostgreSQL integration tests (86 unique .NET tests total), alongside the separate 5-test Playwright suite, with a zero-warning, zero-error build.
- Hardened URL generation, compatibility-route pagination, feed/sitemap/media cancellation, process-local store eviction, CSP/theme loading, health responses, and deployment/network release-gate documentation.

## [0.5.0] - 2026-08-25

- **Containers and CI/CD:** Added separate multi-stage images for Admin, Blog, and Portfolio, Docker Compose orchestration, health checks, non-root runtime execution, image scanning, SBOM generation, and a release migration job.
- **REST contracts:** Added shared request/response DTOs, versioned `/api/v1` routes, pagination, validation problem details, OpenAPI metadata, and schema drift checks.
- **Persistence:** Added application-layer projections and commands, tenant-safe EF Core persistence, audited lifecycle fields, concurrency tokens, indexes, constraints, and the Phase 3 migration.
- **Security hardening:** Added opaque server-side admin sessions with expiry, rotation, revocation, login throttling, antiforgery protection, tenant isolation, safe markdown/media handling, and security headers. **Superseded for Admin auth:** the session path was subsequently removed and Admin auth is now API-key only — a static `X-Admin-Api-Key` header validated by constant-time comparison, with multi-key rotation and no session cookie, no interactive login, and no antiforgery token flow. Antiforgery is disabled where it appears today (the media upload path and the public error pages), not enforced. Tenant isolation, safe markdown/media handling, and security headers are unaffected. The current posture is stated in [`04-engineering-standards/security-baseline.md`](../04-engineering-standards/security-baseline.md).