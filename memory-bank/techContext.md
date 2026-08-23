# Tech Context

Verified against source 2026-08-29 · Phases 0–5 complete · S30.0–S30.4, S30.6–S30.11 complete · 73 .NET tests + 5 Playwright browser tests · 5 migrations · 0-warn/0-error build

## Runtime and projects

- .NET 10 / C# with nullable reference types and implicit usings;
  `TreatWarningsAsErrors=true` and latest analyzers in `Directory.Build.props`.
- 11 projects: AppHost, ServiceDefaults, Core, Infrastructure, Application,
  Contracts, Api.Admin, Web.Blog, Web.Portfolio, Web.Shared, and Tests.
- Aspire AppHost SDK 13.5.2; Aspire.Hosting.PostgreSQL 13.5.3; the dev-only
  `admin-ui` npm resource uses Aspire.Hosting.NodeJs pinned at 9.5.2 (the latest
  published version; there is no 13.x release).
- EF Core / ASP.NET Core aligned at 10.0.11, Npgsql.EntityFrameworkCore.PostgreSQL
  10.0.3. xUnit 2.9.3, Microsoft.NET.Test.Sdk 18.9.0,
  Microsoft.EntityFrameworkCore.InMemory 10.0.11, Microsoft.AspNetCore.TestHost
  10.0.11, Markdig, and Npgsql complete the relevant stack.

## Persistence

- PostgreSQL database `papyrus`, context `PapyrusDbContext`, connection name
  `papyrus`; `Papyrus.Api.Admin` is the sole migration/seed owner.
- Five applied migrations:
  `20260823000000_InitialCreate`,
  `20260825000000_AddProjectCoverImage`,
  `20260825204843_Phase3PersistenceCorrectness`,
  `20260825220000_SecuritySearchTrigramIndexes`, and
  `20260826005202_SyncSearchTrigramIndexesModel`.
- Tenant resolution is scoped and defaults to `main`; EF global tenant and
  soft-delete filters are backed by save-boundary enforcement and composite
  tenant relationships.
- The Admin API rate limiter is per-IP at 120 requests per minute. Infrastructure
  has a fallback development connection string and retries failed database
  operations up to 5 times with a 10-second delay; tests use the in-memory path.

## Authentication and configuration

- API key header: `X-Admin-Api-Key`; production value is supplied as
  `AdminApi__ApiKey` and has no checked-in production default (fail-fast in
  non-Development). The AppHost injects a dev key for local runs.
- Admin UI: React 19 SPA served at `/admin`; protected API requests use the
  `X-Admin-Api-Key` and `X-Tenant-Id` headers, and there is no browser session
  cookie.
- Media is capped at 5 MB and stored under tenant-scoped paths.
- Production must provide `Site__PublicBaseUrl` for absolute canonical, RSS,
  and sitemap URLs; `Site__MediaBaseUrl` and other `Site__*` values are also
  deployment configuration.

## Orchestration and deployment

- Aspire starts PostgreSQL, admin-api, admin-ui (dev-only, port 5173), blog, and
  portfolio. Blog and Portfolio wait for admin-api to become healthy and never
  run migrations. Docker Compose uses PostgreSQL 17 and durable database/media
  volumes.
- Three multi-stage, non-root images are checked in: admin (with a Node build
  stage producing `wwwroot/admin`), blog, and portfolio.
- CI/release workflows build, test, scan, generate SBOMs, check OpenAPI drift,
  run a dedicated migration job, then deploy immutable image tags. `release.yml`
  pins the `dotnet-ef` CLI at 10.0.11.
- Anonymous `/health` and `/alive` endpoints exist, but deployment ingress must
  restrict them to orchestrator/monitoring networks.

## Telemetry (S30.6)

- `Papyrus.ServiceDefaults.Telemetry` owns the `Papyrus` ActivitySource, W3C
  trace-ID configuration, request middleware, telemetry options, and structured
  `IAuditEventSink`.
- Audit events are logger-backed and independent of OTLP availability. API-key
  authentication and Admin mutation/error paths emit tenant-scoped events; API
  keys, cookies, request bodies, and raw secrets are not recorded. Blog and
  Portfolio use the shared request activity seam.
- S30.7 adds `Redactor` at the structured audit sink seam, a bounded
  `CardinalityGuard`, and `TelemetrySamplingPolicy` driven by
  `Telemetry__CardinalityLimit` and `Telemetry__SamplingRate`. Sampling is only
  applied to request diagnostics; logger-backed audit events are always emitted.

## Verification baseline

- Full `dotnet build Papyrus.sln`: 0 warnings and 0 errors.
- Full `dotnet test Papyrus.sln`: 60 passing tests, including S30.3 handler and
  migration-drift tests plus S30.6 telemetry seam tests.
- Phase 4 PostgreSQL projection SQL, latency, and `EXPLAIN (ANALYZE, BUFFERS)`
  evidence is complete for the scoped blocker.
