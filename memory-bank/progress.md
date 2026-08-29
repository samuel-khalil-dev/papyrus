# Progress

Verified against source 2026-08-29 · Phases 0–5 complete · Phase 6 complete (S30.0–S30.12) · 73 .NET tests + 5 Playwright browser tests · 5 migrations · 0-warn/0-error build

## Done

- Release workflow validation/remediation complete: Trivy now uses the immutable
  commit for verified `aquasecurity/trivy-action` v0.33.1; SBOM generation uses
  the immutable commit for verified `anchore/sbom-action` v0.24.1; unused
  `security-events: write` was removed; the dependency-scan Python here-document
  was closed with its missing `PY` terminator. Dockerized `actionlint` and
  `git diff --check` pass.

- Phases 0–5: audit/guardrails, tenant isolation and API-key auth, HTTP/media
  security, application/persistence correctness, REST/projections, and
  containers/CI/CD.
- 11-project .NET 10 modular monolith with Aspire, PostgreSQL, Admin, Blog,
  Portfolio, Web.Shared, Application, Contracts, Infrastructure, Core,
  ServiceDefaults, and Tests.
- C1–C4 implemented: tenant context/filters/save enforcement; API-key-only
  admin authentication; header-authenticated mutation protection; safe raster
  media handling and reference-checked deletion.
- Five migrations defined, including
  `20260825220000_SecuritySearchTrigramIndexes` and
  `20260826005202_SyncSearchTrigramIndexesModel`; `main` seed verified.
- REST `/api/v1`, paginated compatibility `/api`, DTO projections, OpenAPI,
  RSS/sitemaps, security headers, rate/request limits, and CI/release workflows.
- Three multi-stage non-root Dockerfiles and PostgreSQL 17 Compose deployment.
- Phase 4 PostgreSQL projection evidence complete for the scoped latency blocker.
- Security audit redo/remediation complete: no critical findings; all
  W-NEW-01…05 and O-NEW-01…05 fixed for current scope.
- Documentation cleanup complete: W1–W5, O1–O3, O5, and O6 were verified against
  current implementation and removed from active risk acceptance. W6–W8, O4,
  and O7–O10 remain operational/release-gate items.
- Admin React SPA is integrated into the Admin API at `/admin`;
  `Papyrus.Admin.UI` is registered as a dev-only `admin-ui` npm resource in
  Aspire (port 5173, `/api/v1` proxy). All legacy admin Razor pages and the
  cookie-gate session model were retired under ADR 0006.
- `Papyrus.Api.Admin` explicitly references `Microsoft.EntityFrameworkCore.Design`
  10.0.11 with `PrivateAssets=all`, resolving EF CLI startup-project tooling
  errors.
- 32 tests pass; full build passes with 0 warnings and 0 errors under
  `TreatWarningsAsErrors=true`. Baseline dropped from 40 to 32 when
  `AdminSessionTests.cs` was removed with the Razor retirement.
- EF Core package versions are intentionally heterogeneous: Core/Infrastructure
  and ASP.NET/OpenAPI use 10.0.11, Npgsql.EntityFrameworkCore.PostgreSQL uses
  10.0.3, Aspire.Hosting.NodeJs is pinned at 9.5.2, and
  Microsoft.NET.Test.Sdk is 18.9.0.
- Phase 5 completion is evidenced by `release.yml` pinning the `dotnet-ef` CLI
  at 10.0.11, immutable image tags, and a dedicated `migrate` job.
- S30.0 in progress: added the `NetArchTest.Rules` 1.3.2 package and structural
  layer/migration ownership test files under `src/Papyrus.Tests/Architecture`;
  completed with 8 architecture tests passing and 40/40 total tests passing.
- S30.1 complete: added the PostgreSQL integration fixture/tests under
  `src/Papyrus.Tests/Integration`, a `TestDb.CreatePostgres()` seam, the pinned
  Npgsql test reference, Compose `test-db` profile, and the CI PostgreSQL
  service-container path. Verification is green: 6/6 integration tests and
  46/46 solution tests; the solution build has 0 warnings and 0 errors.
- S30.6 complete: added the shared `Papyrus` W3C ActivitySource, telemetry
  module/request seam, logger-backed structured audit sink, and audit emissions
  for API-key authentication plus Admin API mutations/errors. Admin, Blog, and
  Portfolio use the common request seam. No persisted audit table or migration
  was needed. Verification: 49/49 solution tests and a clean build.
- S30.3 complete: added application handler tests for content command/query,
  public query, taxonomy, and lifecycle P0/P1 paths, plus migration-drift tests
  for EF pending model changes and migration snapshot/create-script consistency.
  Verification: 60/60 solution tests and a clean zero-warning build. No
  production or applied migration files were modified.
- S30.7 complete: added fail-closed audit redaction, bounded diagnostic tag
  cardinality, configurable request diagnostic sampling, and telemetry tests
  proving redacted output and audit retention at 0% sampling. Verification:
  63/63 solution tests and a clean zero-warning build; no migrations changed.
- S30.2 complete: expanded the existing PostgreSQL fixture collection to 14
  tests covering Npgsql-specific search/index behavior, transactions,
  concurrency, tenant/soft-delete filters, and no-tracking projection shape.
  No migrations changed.
- S30.4 complete: added strict Playwright 1.55.0 smoke specs for the Admin SPA,
  versioned API authentication, public Blog, and public Portfolio. CI runs the
  non-required browser job with PostgreSQL 17 and Chromium. Verification: 5/5
  browser tests passed.
- S30.9/S30.10 complete: added `docs/performance/phase6-baseline.md`, a manual
  `.github/workflows/perf-baseline.yml` dispatch workflow, K6 response-payload
  metrics, and PostgreSQL command-count budget tests. The baseline records public
  feed DB p95 of 0.073 ms (Blog) and 0.041 ms (Portfolio), four public queries,
  two admin collection queries, and explicit latency/error/payload/upload/
  allocation/GC budgets. No production optimization was applied because the
  measured paths are already async, projected, no-tracking, bounded, and free of
  N+1/sequential-scan evidence.
- S30.8 complete: added the portable Grafana/Prometheus dashboard, nine-family
  alert runbook, and telemetry-runbook cross-reference. Dashboard signals use
  existing telemetry and collector-derived views where application counters do
  not exist.
- S30.11 complete: added incident and telemetry runbooks, corrected documentation
  to the 73 .NET plus 5 Playwright test baseline, and attached W6, W7, W8, O4,
  O8, and O10 release-gate evidence. O7 remains pending S30.5 load evidence.
- S30.5 completed on 2026-08-29 against the Docker Compose PostgreSQL 17
  deployment. Public aggregate K6 p95/payload p95 were 23.574 ms/9,030 bytes;
  admin values were 21.660 ms/9,256 bytes. Both runs had 0.00% failed requests
  and 100.00% checks; admin recorded zero 5xx responses. Dated raw summaries
  were added under `docs/performance/`, and O7 now has load evidence attached;
  restore/migration/media drills remain open.
- S30.12 complete: created
  `docs/architecture/decisions/0007-postgres-rls-decision.md` recording the
  decision to DECLINE Postgres RLS; application-level tenant filtering remains
  the sole enforcement layer. Phase 6 is now fully complete (S30.0–S30.12).
- Managed Admin API-key rotation complete: added the reloadable
  `AdminApi:ApiKeys` configuration set, retained `AdminApi:ApiKey` compatibility,
  used fixed-time comparison across all configured candidates, and added four
  xUnit tests for acceptance, rejection, binding, and add/overlap/remove
  rotation behavior. The solution build passed with 0 warnings and 0 errors;
  the full test run passed 61 tests and had 16 PostgreSQL connection failures
  because 127.0.0.1:5432 was unavailable.

## Not started / open

- Phase 6 is complete (S30.0–S30.12).
- Phase 7: release readiness and showcase documentation/assets.
- Phase 8: measured scale decision gate.
- O7 is complete: S30.5 load evidence plus the 2026-08-29 Compose restore,
  migration-ownership, and media-volume drills are recorded in the release gate
  and incident runbook. The media drill required non-root ownership setup for
  `/data/media` in `Dockerfile.admin`; the rebuilt image passed the restart test.
- Operate the managed API-key rotation procedure with a reloadable secret
  provider before broader multi-operator or replica-scale deployment.
- Production `Site:PublicBaseUrl` configuration completed: Compose requires the
  per-site public URL inputs, the release workflow passes them from the
  production environment, and deployment documentation describes the binding
  and absolute canonical/RSS/sitemap generation flow. Exact hostnames remain
  deployment-owned.
- `docs/guides/contributing.md` and `docs/security/policy.md` are still
  placeholders pending Phase 7.

## Verification history

- 2026-08-29: Memory bank reconstructed against current source. Confirmed
  `Papyrus.Api.Admin` and `Papyrus.Web.Portfolio` exist; five migrations;
  32-test baseline; AppHost wiring (admin-api migrator, admin-ui dev-only, blog/
  portfolio wait on admin-api). No source or migration changes were made.
- 2026-08-28: Completed the `admin-ui` Aspire DCP startup fix
  (`WithHttpEndpoint(targetPort: 5173, name: "http")`). Build clean.
- 2026-08-28: Applied the only approved upgrade, `Microsoft.NET.Test.Sdk`
  17.13.0 → 17.14.1, and later verified current version 18.9.0; 32/32 tests;
  Admin UI build/lint and `git diff --check` passed.
- 2026-08-28: Resolved the Aspire NodeJs package-version blocker by pinning
  9.5.2 (latest published; no 13.x); marked `plans/wire-admin-ui-to-aspire.md`
  Implemented; corrected the target port to 5173.

- 2026-08-29: S30.5 completed; K6 load evidence attached for O7. S30.12
  completed; Postgres RLS decision ADR records DECLINE RLS.
- 2026-08-28: Fixed the Admin API 401 under Aspire by injecting
  `AdminApi__ApiKey` via `.WithEnvironment(...)` in `Papyrus.AppHost/Program.cs`.
- 2026-08-26: Resolved EF pending model changes by generating
  `20260826005202_SyncSearchTrigramIndexesModel`; verified no pending changes
  and a 0-warning/0-error build.
- 2026-08-26: Retired the admin Razor UI and cookie-gate session model under
  ADR 0006; 32/32 tests and a clean build.
- 2026-08-26: Verified the Admin SPA install/build/lint via `npm.cmd` with
  0 vulnerabilities and 0 lint warnings/errors.
- 2026-08-26: Documented the decision to wire the React Admin SPA into Aspire as
  a dev-only `AddNpmApp` resource (`plans/wire-admin-ui-to-aspire.md`).

## Admin SPA scaffold (retained history)

- Scaffolded `src/Papyrus.Admin.UI` with React 19 / Vite / TypeScript strict
  stack, Tailwind v4, shadcn-style primitives, React Query, openapi-fetch, forms,
  TipTap (`@tiptap/markdown`), icons, and animations.
- Added the typed `/api/v1` API wrapper injecting `X-Admin-Api-Key` and
  `X-Tenant-Id` with 401/403/429 event handling, plus backend-aligned frontend
  DTO interfaces and placeholder schema.
- Implemented the dashboard, post/project CRUD tables and Zod/RHF forms, media
  grid/upload/delete flows, responsive shell, command palette, and shared
  feedback/status/pagination/raster-dropzone components.
- Vite base is `/admin/`; the backend serves the SPA from `wwwroot/admin`
  (published) or the sibling `dist` (dev), with SPA-shell route fallback.
- Dependency/verification blockers (PowerShell npm shim, `@tiptap/extension-markdown`
  404) were resolved; `npm.cmd` is the working shim and the lockfile is tracked.
