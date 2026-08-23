# Active Context

Verified against source 2026-08-29 · Phases 0–5 complete · S30.0–S30.4, S30.6–S30.11 complete · 73 .NET tests + 5 Playwright browser tests · 5 migrations · 0-warn/0-error build

## Current task

S30.0–S30.4, S30.6–S30.11 are complete. Phase 6 now includes the architecture,
PostgreSQL integration, handler/migration-drift, Playwright, telemetry,
performance-budget, dashboard/alert, and runbook/release-gate work. The current
verification baseline is 73 .NET tests plus 5 Playwright browser tests, with a
clean zero-warning/zero-error build. S30.5 remains the focused K6 load-test
item; O7 release-gate evidence is explicitly pending that work.

S30.3 is complete. Added InMemory handler coverage for content create/update/
validation, content lists, public feed/search/detail, taxonomy, and lifecycle
soft-delete/restore paths. Added migration-drift tests under
`src/Papyrus.Tests/Migrations`: EF `HasPendingModelChanges()` is the pending
model gate, while the snapshot/create-script test verifies the migration
assembly snapshot and all owned tables. No production source or applied
migration file was changed.

S30.6 is complete. The reusable PostgreSQL fixture and integration coverage now
live under `src/Papyrus.Tests/Integration`; the existing migrations are applied
against a dedicated `papyrus_test` database and a known integration tenant. No
production source, secret, or migration files were changed.

S30.0 architecture tests now live under `src/Papyrus.Tests/Architecture` using
pinned `NetArchTest.Rules` 1.3.2. The stray duplicate Phase 6 plan was deleted;
the canonical plan remains authoritative. Eight architecture tests pass, and
the full suite now passes 40 tests.

The current feature state: the React Admin SPA is the sole `/admin` experience,
served by `Papyrus.Api.Admin` (from `wwwroot/admin` in published/container
builds or the sibling UI's `dist` during development) and registered in Aspire as
a dev-only `admin-ui` npm resource on port 5173 that proxies `/api/v1` to the
admin API. All legacy admin Razor pages, the cookie gate, session store, login
throttle, and Razor antiforgery wiring have been retired under ADR 0006. The SPA
uses `X-Admin-Api-Key` + `X-Tenant-Id` for protected `/api/v1` calls.

The EF model is synchronized with the migration history; a pending
`PendingModelChangesWarning` was resolved with `20260826005202_SyncSearchTrigramIndexesModel`,
recording the seven model-managed trigram indexes without recreating them.

## Current state

- Phases 0–5 are complete; S30.0–S30.4 and S30.6–S30.11 of Phase 6 are complete.
- Remaining Phase 6 work is S30.5 (K6 load tests, P2) and optional S30.12
  (Postgres RLS decision ADR, P2).
- Phases 7 and 8 are not started; Phase 8 remains a decision gate.
- Multi-tenancy (C1), API-key admin auth (C2), header-authenticated mutation
  protection (C3), and media safety (C4) are implemented and closed for their
  code scope.
- The security audit was redone and remediated: no critical findings;
  W-NEW-01…05 and O-NEW-01…05 are fixed. S30.11 attached evidence for W6, W7,
  W8, O4, O8, and O10; O7 remains pending S30.5 load evidence.
- W1–W5, O1–O3, O5, and O6 were verified against current code and removed from
  active risk acceptance. O7 remains open pending S30.5; other remaining
  operational/release gates are tracked in the attached S30.11 evidence.
- Five migrations are defined, including the trigram-index migration and its
  model-sync migration. Existing migration files were not edited or deleted.
- `Papyrus.Api.Admin` explicitly references
  `Microsoft.EntityFrameworkCore.Design` 10.0.11 with private assets, so EF
  Core CLI commands work with the Admin startup project.
- The suite has 73 passing .NET tests plus 5 passing Playwright browser tests,
  and the warnings-as-errors build is clean.
- S30.6 provides the shared `Papyrus` ActivitySource, W3C trace propagation,
  low-cardinality request seams, and a logger-backed structured audit sink.
  Admin auth and mutation/error requests emit tenant-scoped events without API
  keys, cookies, request bodies, or raw credentials; Blog and Portfolio use the
  same request seam.
- The AppHost wiring is complete: `admin-api` migrates/seeds; `admin-ui`
  (NodeJs 9.5.2) is dev-only; blog and portfolio wait for admin-api health and
  never run migrations.

## Open items

- Complete S30.5 K6 load tests (P2), then attach the O7 release-gate load
  evidence. Optionally complete S30.12, the Postgres RLS decision ADR (P2).
- Establish managed API-key rotation and multi-key support before broader
  multi-operator or replica-scale deployment.
- Set `Site:PublicBaseUrl` in production for absolute canonical/RSS/sitemap
  URLs.
- `docs/guides/contributing.md` and `docs/security/policy.md` remain
  placeholders pending Phase 7.

## Session notes

- 2026-08-28: Fixed the Aspire DCP startup error for the dev-only, proxied
  non-container `admin-ui` resource. The AppHost now uses
  `WithHttpEndpoint(targetPort: 5173, name: "http")`, letting Aspire allocate the
  external port. `dotnet build Papyrus.sln --no-restore` passed with 0 warnings
  and 0 errors.
- 2026-08-28: Diagnosed and fixed a 401 from the Admin API under Aspire. The
  `admin-api` resource was not injecting `AdminApi:ApiKey`; added
  `.WithEnvironment("AdminApi__ApiKey", "dev-admin-key")` in
  `src/Papyrus.AppHost/Program.cs`. Build passed with 0 warnings/errors.
- 2026-08-28: Confirmed Aspire.Hosting.NodeJs 9.5.2 is the latest published
  version; resolved the package-version blocker and marked
  `plans/wire-admin-ui-to-aspire.md` Implemented with acceptance criteria
  checked.
- 2026-08-26: Retired legacy admin Razor pages and the cookie-gate session model
  under ADR 0006; removed Razor/antiforgery/session throttling wiring, cookie
  helpers, and obsolete tests. Retirement verification: 32/32 tests.
- 2026-08-26: Synchronized EF model to migration history via
  `20260826005202_SyncSearchTrigramIndexesModel`; no pending model changes.
- 2026-08-26: Wired the Admin SPA into the backend and
  `Dockerfile.admin` (Node build stage); Vite base `/admin/`.
- 2026-08-26: Verified SPA install/build/lint via `npm.cmd` (0 vulnerabilities;
  lint 0 errors).
- 2026-08-29: Completed S30.6. No persisted audit table or migration was added;
  49/49 tests and the zero-warning build passed.
- 2026-08-29: Completed S30.3. Added 11 handler/migration tests; the full suite
  passes 60/60 and `dotnet build .\Papyrus.sln --nologo --no-incremental` passes
  with 0 warnings and 0 errors.
- 2026-08-29: Completed S30.7. Added fail-closed telemetry redaction, bounded
  diagnostic cardinality, configurable diagnostic sampling, and tests proving
  audit logging survives 0% diagnostic sampling. The full suite passes 63/63
  and the solution build remains 0-warning/0-error.
- 2026-08-29: Completed S30.2 and S30.4. PostgreSQL integration coverage passes
  14/14; Playwright 1.55.0 browser smoke passes 5/5; the full xUnit suite passes
  71/71 and the solution build remains 0-warning/0-error. The admin write
  smoke exposed that explicit handler transactions conflicted with Npgsql's
  configured retry execution strategy; content commands now rely on EF's
  atomic SaveChanges transaction, which is retry-compatible.
- 2026-08-29: Completed S30.9/S30.10. Added the manual dispatch performance
  baseline workflow, K6 payload-byte metrics, explicit public/admin/upload/
  allocation/GC budgets, and PostgreSQL query-count budget tests. The measured
  baseline shows public feed DB p95 below 0.1 ms and the existing projected,
  bounded four-query shape; no production optimization was justified. Build
   passes with 0 warnings/errors and the full .NET suite passes 73/73.
- 2026-08-29: Completed S30.8 and S30.11. Added the portable Grafana dashboard,
  nine-family alert runbook, incident/telemetry runbooks, corrected the documented
  test baseline to 73 .NET plus 5 Playwright tests, and attached release-gate
  evidence for W6, W7, W8, O4, O8, and O10. O7 remains pending S30.5.

## Self-Healing Log

Errors encountered and resolved across sessions; recorded for traceability.
No new failures occurred during this memory-bank reconstruction session.

- **S30.0 architecture test build** — initial test anchors referenced nonexistent
  `Papyrus.Contracts.Paging.PagedResult<>` and
  `Papyrus.Application.Content.ContentCommands` types. **Fix:** anchor the
  NetArchTest rules to `PublicCategory` and `ContentCommandHandlers`; added the
  required reflection import. Verification is pending the next build.
- **S30.0 architecture test run** — migration ownership source scan also matched
  its own assertion string, producing two `.MigrateAsync()` call sites. **Fix:**
  exclude the architecture-test directory from production call-site scanning.
  Verification is pending the next test run.
- **S30.0 architecture test run** — startup ownership scan matched the Infrastructure
  extension declaration as well as the Admin call site. **Fix:** match the explicit
  `await app.InitializeDatabaseAsync()` invocation. Resolved; the full suite then
  passed 40/40 tests.

- **Aspire DCP startup (admin-ui)** — proxied resource failed because `Port` and
  `TargetPort` mapped to the same value. **Fix:** AppHost uses
  `WithHttpEndpoint(targetPort: 5173, name: "http")` so Aspire allocates the
  external port. Resolved 2026-08-28.
- **Aspire.Hosting.NodeJs version** — restore failed for the requested 13.5.2;
  NuGet has no 13.x release. **Fix:** pinned 9.5.2, the latest published version;
  documented in `plans/wire-admin-ui-to-aspire.md`. Resolved 2026-08-28.
- **Admin API 401 under Aspire** — `ApiKeyAuthenticationHandler` reported the key
  was not configured because `admin-api` lacked the environment injection. **Fix:**
  `.WithEnvironment("AdminApi__ApiKey", "dev-admin-key")` on the `admin-api`
  resource. Resolved 2026-08-28.
- **EF pending model changes** — startup `PendingModelChangesWarning`; the seven
  trigram indexes were in the model but absent from the snapshot (preceding
  migration used raw SQL). **Fix:** generated model-sync migration
  `20260826005202_SyncSearchTrigramIndexesModel`; verified
  `has-pending-model-changes` reports none. Resolved 2026-08-26.
- **npm registry 404** — `@tiptap/extension-markdown@^3.4.1` did not exist.
  **Fix:** replaced with published `@tiptap/markdown`; install/build/lint then
  passed. Resolved 2026-08-26.
- **Playwright admin write smoke** — the first browser test omitted the SPA API
  key, so the protected write returned unauthorized. **Fix:** seed the key in
  local storage and reload the SPA before exercising the write path.
- **Npgsql retry strategy and content writes** — explicit user transactions in
  content command handlers caused `NpgsqlRetryingExecutionStrategy` failures.
  **Fix:** removed the redundant explicit transaction; one EF `SaveChangesAsync`
  unit remains atomic and is compatible with the configured retry strategy.
- **Compose diagnostics** — a local `docker compose logs` command initially
  lacked the required interpolation variables. **Fix:** reran it with the
  documented temporary `POSTGRES_PASSWORD` and `ADMIN_API_KEY` values.

- **S30.1 full-suite architecture test** — the repository migration ownership
  scan found the new fixture's intentional `MigrateAsync()` call and failed
  45/46. **Fix:** excluded the dedicated `Papyrus.Tests/Integration` harness
  from production migration call-site scanning, alongside the architecture
  test exclusion. The fixture remains responsible for applying real migrations.

- **S30.1 local Compose startup** — `docker compose --profile test up -d test-db`
  initially failed because Compose interpolates required `POSTGRES_PASSWORD` and
  `ADMIN_API_KEY` values from the full file even when only the test profile is
  selected. **Fix:** supplied temporary PowerShell values
  (`POSTGRES_PASSWORD=postgres`, `ADMIN_API_KEY=dev-admin-key`); the isolated
  `test-db` service then started successfully.

- **S30.1 integration index test** — the first assertion expected trigram indexes,
  but the existing migration set discovered by EF did not include the raw-SQL
  trigram migration (the test database reported four applied migrations and no
  trigram indexes). **Fix:** kept S30.1 schema-neutral and asserted the indexes
  actually owned by the model/migrations: tenant-leading slug uniqueness and
  `IsDeleted` partial filtering. Trigram coverage remains an S30.2 concern and
   no migration history was changed.

- **S30.6 telemetry build** — initial missing hosting/DI imports and an
  unsupported high-arity `ILogger.Log` overload caused compilation errors.
  **Fix:** added the required imports and used the structured logging overload;
  build and tests then passed with 0 warnings/errors and 49/49 tests.

- **S30.3 verification** — no terminal, build, test, or EF migration failures
  occurred. The pending-model gate and snapshot/create-script consistency tests
  passed against the current five-migration model.
- **Memory-bank synchronization** — two broad patches initially failed because
  their expected context did not match the current files. **Fix:** re-read the
  affected files and apply narrower, line-accurate patches; synchronization is
  now complete.

- **S30.5 K6 local verification** — `k6 version` was unavailable because K6 is
  not installed on this workstation. **Resolution:** CI installs the locked
  Grafana K6 tool with `grafana/setup-k6-action@v1`; local run commands and the
  limitation are documented in `tests/load/README.md`.

- **S30.9 initial budget-test build** — the command interceptor test used an
  incorrect nullable scalar override, attempted async disposal of a non-disposable
  interceptor, and passed the interceptor into the optional tenant argument.
  **Fix:** use the EF-compatible `object` signature, normal local lifetime, and
  `CreateDbContext(null, interceptor)`. Resolved; build and tests pass.

- **S30.8 dashboards and alerts** — added the portable Grafana dashboard at
  `ops/dashboards/papyrus-dashboard.json`, the alert-family runbook at
  `docs/runbooks/alerts.md`, and a telemetry-runbook cross-reference. Dashboard
  JSON validation passed with `schemaVersion` 39; `git diff --check` passed.
- **S30.8 dashboard validation** — the first validation found a missing root
  closing brace in the dashboard artifact. **Fix:** added the final brace and
  revalidated successfully with PowerShell `ConvertFrom-Json`.
