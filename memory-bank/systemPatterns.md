# System Patterns

Verified against source 2026-08-29 · Phases 0–5 complete · Phase 6 complete (S30.0–S30.12) · 73 .NET tests + 5 Playwright browser tests · 5 migrations · 0-warn/0-error build

## Architecture

Papyrus is a modular monolith with three runnable web apps, one PostgreSQL
database, and 11 solution projects. AppHost coordinates Admin API/UI, Blog, and
Portfolio. The Admin API owns writes, migration, seed, authentication, and media;
the public hosts are read-only. The React admin SPA is a dev-only `AddNpmApp`
resource in Aspire but is served same-origin from the Admin API at `/admin` in
production.

## Layering

- Core: entities, tenant marker, and hardened Markdown rendering.
- Application: validated commands, queries, and no-tracking public projections.
- Contracts: explicit REST request/response DTOs and pagination envelopes.
- Infrastructure: EF mappings, tenant/soft-delete filters, save enforcement,
  migrations, seed, and database adapters.
- Delivery: Admin API/UI and the two public Razor sites; Web.Shared contains
  shared public chrome and URL/feed helpers.

## Tenant and persistence pattern

`TenantContext` resolves `X-Tenant-Id` or `Tenant:DefaultTenantId`, defaults to
`main`, and validates `^[a-z0-9][a-z0-9-]{0,62}$`. EF global query filters,
tenant-leading composite relationships, save-boundary validation, and
tenant-scoped media prevent cross-tenant access. Soft-deleted rows are excluded
by default.

## Authentication pattern

The API and React SPA use `X-Admin-Api-Key` with fixed-time comparison plus
`X-Tenant-Id`. The SPA is served at `/admin` and sends the key on protected
`/api/v1` requests. Legacy admin Razor pages, the cookie gate, the session store,
and login throttling were retired under ADR 0006; there is no browser session
cookie.

## Public query pattern

Public queries are asynchronous, no-tracking DTO projections bounded by
pagination and live-content predicates. Search uses PostgreSQL `ILIKE` with
bounded input and trigram indexes. Feeds and sitemaps use the same tenant/live
predicates and bounded materialization.

## Security and operations

Header-authenticated admin mutations require API authorization. Uploads are
raster-only, magic-byte validated, capped at 5 MB, atomically written, and cannot
delete referenced files. CSP excludes `unsafe-inline`; security headers,
multipart/rate limits, minimal health responses, and cancellation-aware paths
are active. The Admin API rate limiter is per-IP at 120 requests per minute.
Infrastructure has a fallback development connection string and retries failed
database operations up to 5 times with a 10-second delay; tests use the in-memory
path. Health endpoints remain anonymous but must be network-restricted in
production.

## Roadmap state

Phases 0–5 are complete. Phase 6 is complete (S30.0–S30.12), including the K6
load evidence (S30.5) and the Postgres RLS decision ADR (S30.12, DECLINE RLS).
Phases 7 and 8 are not started; Phase 8 is a measured decision gate. W6–W8, O4,
and O7–O10 remain release-gate verifications, not blanket-accepted code defects.
O7's load-evidence portion is done; O7 remains open only pending the restore,
migration-ownership, and media-volume drills.

## Observability

- All delivery hosts use the shared `Papyrus.ServiceDefaults.Telemetry` request
  seam and W3C trace IDs. The single `Papyrus` ActivitySource is registered in
  the existing OpenTelemetry pipeline and retains the OTLP endpoint gate.
- Admin API-key authentication and mutation/error requests publish structured,
  tenant-scoped audit events through `IAuditEventSink`. Audit logging remains
  active without an OTLP listener and is not subject to diagnostic sampling;
  S30.7 adds fail-closed redaction, bounded diagnostic tag cardinality, and
  configurable sampling for high-volume request diagnostics. Audit events are
  redacted at the sink seam and always logged.
