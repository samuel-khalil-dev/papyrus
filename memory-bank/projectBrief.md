# Project Brief

Verified against source 2026-08-29 · Phases 0–5 complete · S30.3, S30.6, and S30.7 complete · 63 tests · 5 migrations · 0-warn/0-error build

Papyrus is a .NET 10 modular monolith for technical writing and a professional
showcase. It contains a public Blog Razor Pages site, a public Portfolio Razor
Pages site, and an authenticated React Admin SPA served from `Papyrus.Api.Admin`
at `/admin`. The solution has 11 projects and uses PostgreSQL through EF Core
and Npgsql. Aspire coordinates local PostgreSQL, the admin API/UI, and the two
public sites; the admin API is the sole migration/seed owner.

## Goals

- Publish technical posts and showcase portfolio work.
- Relate projects to deeper blog posts.
- Manage content, taxonomy, and media from the React admin surface through the
  versioned `/api/v1` REST API.
- Run locally with Aspire (admin UI included as a dev-only npm resource) and
  deploy three non-root web containers with PostgreSQL.

## Implemented boundaries

- `Papyrus.Core` owns domain entities and safe Markdown rendering.
- `Papyrus.Application` owns validated commands, queries, and projections.
- `Papyrus.Contracts` owns REST DTOs and pagination contracts.
- `Papyrus.Infrastructure` owns EF Core, PostgreSQL, migrations, seed, tenant
  filters, and persistence enforcement.
- `Papyrus.Api.Admin` owns writes, API-key authorization, the SPA serving path,
  and tenant-scoped media; it is the single migrator/seed owner.
- Blog and Portfolio are read-only public Razor sites; `Papyrus.Web.Shared` owns
  shared public presentation and URL/feed helpers.

## Current security and data state

- Tenant resolution uses `X-Tenant-Id` or `Tenant:DefaultTenantId`, defaults to
  `main`, and validates `^[a-z0-9][a-z0-9-]{0,62}$`.
- EF tenant/soft-delete global filters and save-boundary checks are active.
- The admin API uses fixed-time `X-Admin-Api-Key` comparison for `/api/v1`; the
  SPA has no browser session cookie. The legacy cookie gate, login throttle, and
  Razor antiforgery wiring were retired under ADR 0006.
- Raster-only media validation (5 MB cap), safe Markdown, security headers,
  per-IP rate limiting (120/min), request limits, and production API-key
  fail-fast behavior are implemented.
- Five migrations are applied through
  `20260826005202_SyncSearchTrigramIndexesModel`; the `main` seed is verified.
  Applied migration files are not edited or deleted.

## Roadmap state

- Phases 0–5: **complete**.
- Phase 6 (observability, architecture/test depth, PostgreSQL/integration/
  browser/load testing, and performance budgets): **in progress**; S30.0,
  S30.1, S30.3, S30.6, and S30.7 are complete.
- Phase 7 (release readiness/showcase): **not started**.
- Phase 8 (measured scale): **not started; decision gate**.

The current verification baseline is 63 passing tests and a clean
warnings-as-errors build. `docs/guides/contributing.md` and
`docs/security/policy.md` remain Phase 7 placeholders. Production must set
`Site:PublicBaseUrl` for absolute canonical/RSS/sitemap URLs. Session and
login-throttle stores were removed; authentication is header-key based and
process-local, so managed API-key rotation before multi-operator or replica-scale
deployment remains open.
