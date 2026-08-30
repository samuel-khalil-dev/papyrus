# System Patterns

## Modular monolith and layering

Core owns entities and safe Markdown. Application owns CQRS commands/queries and projections. Contracts owns explicit REST DTOs and pagination. Infrastructure owns EF mappings, filters, persistence enforcement, migrations, and seed. Admin owns authenticated writes, media, SPA serving, and the sole startup migration/seed call. Blog and Portfolio are read-only delivery hosts.

## Tenant and persistence pattern

Scoped `TenantContext` resolves `X-Tenant-Id`, defaults to `main`, and validates `^[a-z0-9][a-z0-9-]{0,62}$`. EF global filters apply tenant and soft-delete predicates. `EnforceTenantBoundary()` validates tracked changes at save time, tenant-leading composite relationships prevent cross-tenant associations, and media uses `{root}/{tenantId}/`. Application-level enforcement is the sole layer; RLS is not used.

## CQRS and projections

Application separates commands from queries. `PublicContentQueries` returns asynchronous, bounded, no-tracking DTO projections with live-content predicates. Search uses bounded PostgreSQL `ILIKE` and trigram indexes. Feeds and sitemaps reuse tenant and publication rules.

## Audit and soft delete

`PapyrusSaveChangesInterceptor` sets audit timestamps and actors and converts deletes into soft deletes. Global filters exclude soft-deleted rows by default.

## API-key authentication

`ApiKeyAuthenticationHandler` reads `X-Admin-Api-Key` and uses constant-time `AdminApiKey.MatchesAny` comparison. Reloadable `AdminApi:ApiKeys` supports add/overlap/remove rotation; `AdminApi:ApiKey` is a compatibility alias. Production fails fast when no key is configured or it is `change-me`.

## Telemetry

The shared seam uses the `Papyrus` ActivitySource and W3C propagation. `TelemetryRequestMiddleware` creates bounded request activities. `StructuredAuditEventSink` emits tenant-scoped audit events independently of OTLP and diagnostic sampling. `Redactor` fails closed, `CardinalityGuard` defaults to 100, and `TelemetrySamplingPolicy` samples diagnostics only at a default rate of 1.0. Keys, cookies, bodies, and raw secrets are never recorded.

## Sole migrator

Infrastructure owns migration capability; `Papyrus.Api.Admin` is the only startup caller through `InitializeDatabaseAsync()`. Blog, Portfolio, and other services never migrate. Seed follows the same Admin-owned initialization path.
